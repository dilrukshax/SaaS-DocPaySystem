using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NotificationService.Domain.Entities;
using NotificationService.Infrastructure.Data;
using NotificationService.Infrastructure.Services;
using Shared.Kernel.Events;
using Shared.Kernel.Services;
using FluentValidation;

namespace NotificationService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly NotificationDbContext _context;
    private readonly IEmailNotificationService _emailService;
    private readonly IEventPublisher _eventPublisher;
    private readonly IValidator<SendNotificationRequest> _sendValidator;
    private readonly ILogger<NotificationsController> _logger;

    public NotificationsController(
        NotificationDbContext context,
        IEmailNotificationService emailService,
        IEventPublisher eventPublisher,
        IValidator<SendNotificationRequest> sendValidator,
        ILogger<NotificationsController> logger)
    {
        _context = context;
        _emailService = emailService;
        _eventPublisher = eventPublisher;
        _sendValidator = sendValidator;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<NotificationDto>>> GetNotifications(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? type = null,
        [FromQuery] string? status = null,
        [FromQuery] string? recipient = null)
    {
        var query = _context.Notifications.AsQueryable();

        if (!string.IsNullOrEmpty(type))
        {
            query = query.Where(n => n.Type == type);
        }

        if (!string.IsNullOrEmpty(status))
        {
            query = query.Where(n => n.Status == status);
        }

        if (!string.IsNullOrEmpty(recipient))
        {
            query = query.Where(n => n.Recipient.Contains(recipient));
        }

        var totalCount = await query.CountAsync();
        var notifications = await query
            .OrderByDescending(n => n.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(n => new NotificationDto
            {
                Id = n.Id,
                Type = n.Type,
                Recipient = n.Recipient,
                Subject = n.Subject,
                Status = n.Status,
                SentAt = n.SentAt,
                CreatedAt = n.CreatedAt
            })
            .ToListAsync();

        Response.Headers.Add("X-Total-Count", totalCount.ToString());
        return Ok(notifications);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<NotificationDetailDto>> GetNotification(Guid id)
    {
        var notification = await _context.Notifications.FindAsync(id);
        if (notification == null)
        {
            return NotFound();
        }

        var dto = new NotificationDetailDto
        {
            Id = notification.Id,
            Type = notification.Type,
            Recipient = notification.Recipient,
            Subject = notification.Subject,
            Body = notification.Body,
            Status = notification.Status,
            ErrorMessage = notification.ErrorMessage,
            Metadata = notification.Metadata,
            SentAt = notification.SentAt,
            CreatedAt = notification.CreatedAt,
            UpdatedAt = notification.UpdatedAt
        };

        return Ok(dto);
    }

    [HttpPost("send")]
    public async Task<ActionResult<NotificationDto>> SendNotification(SendNotificationRequest request)
    {
        var validationResult = await _sendValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.Errors);
        }

        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            Type = request.Type,
            Recipient = request.Recipient,
            Subject = request.Subject,
            Body = request.Body,
            Status = "Pending",
            Metadata = request.Metadata ?? "{}",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();

        // Send the notification
        try
        {
            switch (request.Type.ToLower())
            {
                case "email":
                    await _emailService.SendEmailAsync(
                        request.Recipient,
                        request.Subject,
                        request.Body,
                        request.TemplateId);
                    break;
                // Add other notification types (SMS, Push, etc.) here
                default:
                    throw new NotSupportedException($"Notification type '{request.Type}' is not supported");
            }

            notification.Status = "Sent";
            notification.SentAt = DateTime.UtcNow;
            notification.UpdatedAt = DateTime.UtcNow;
        }
        catch (Exception ex)
        {
            notification.Status = "Failed";
            notification.ErrorMessage = ex.Message;
            notification.UpdatedAt = DateTime.UtcNow;
            
            _logger.LogError(ex, "Failed to send notification {NotificationId}", notification.Id);
        }

        await _context.SaveChangesAsync();

        // Publish notification sent event
        await _eventPublisher.PublishAsync(new NotificationSentEvent
        {
            NotificationId = notification.Id,
            Type = notification.Type,
            Recipient = notification.Recipient,
            Status = notification.Status,
            SentAt = notification.SentAt,
            ErrorMessage = notification.ErrorMessage
        });

        _logger.LogInformation("Notification {NotificationId} sent to {Recipient} with status {Status}",
            notification.Id, notification.Recipient, notification.Status);

        var dto = new NotificationDto
        {
            Id = notification.Id,
            Type = notification.Type,
            Recipient = notification.Recipient,
            Subject = notification.Subject,
            Status = notification.Status,
            SentAt = notification.SentAt,
            CreatedAt = notification.CreatedAt
        };

        return CreatedAtAction(nameof(GetNotification), new { id = notification.Id }, dto);
    }

    [HttpPost("bulk-send")]
    [Authorize(Roles = "Admin,NotificationManager")]
    public async Task<ActionResult<BulkNotificationResponseDto>> SendBulkNotifications(BulkNotificationRequest request)
    {
        var notifications = new List<Notification>();
        var results = new List<NotificationDto>();

        foreach (var recipient in request.Recipients)
        {
            var notification = new Notification
            {
                Id = Guid.NewGuid(),
                Type = request.Type,
                Recipient = recipient,
                Subject = request.Subject,
                Body = request.Body,
                Status = "Pending",
                Metadata = request.Metadata ?? "{}",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            notifications.Add(notification);
        }

        _context.Notifications.AddRange(notifications);
        await _context.SaveChangesAsync();

        // Send notifications in parallel
        var sendTasks = notifications.Select(async notification =>
        {
            try
            {
                switch (request.Type.ToLower())
                {
                    case "email":
                        await _emailService.SendEmailAsync(
                            notification.Recipient,
                            notification.Subject,
                            notification.Body,
                            request.TemplateId);
                        break;
                    default:
                        throw new NotSupportedException($"Notification type '{request.Type}' is not supported");
                }

                notification.Status = "Sent";
                notification.SentAt = DateTime.UtcNow;
                notification.UpdatedAt = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                notification.Status = "Failed";
                notification.ErrorMessage = ex.Message;
                notification.UpdatedAt = DateTime.UtcNow;
                
                _logger.LogError(ex, "Failed to send bulk notification {NotificationId}", notification.Id);
            }

            return new NotificationDto
            {
                Id = notification.Id,
                Type = notification.Type,
                Recipient = notification.Recipient,
                Subject = notification.Subject,
                Status = notification.Status,
                SentAt = notification.SentAt,
                CreatedAt = notification.CreatedAt
            };
        });

        results = (await Task.WhenAll(sendTasks)).ToList();
        await _context.SaveChangesAsync();

        var response = new BulkNotificationResponseDto
        {
            TotalSent = results.Count(r => r.Status == "Sent"),
            TotalFailed = results.Count(r => r.Status == "Failed"),
            Results = results
        };

        _logger.LogInformation("Bulk notification sent: {TotalSent} successful, {TotalFailed} failed",
            response.TotalSent, response.TotalFailed);

        return Ok(response);
    }

    [HttpPost("{id}/retry")]
    public async Task<IActionResult> RetryNotification(Guid id)
    {
        var notification = await _context.Notifications.FindAsync(id);
        if (notification == null)
        {
            return NotFound();
        }

        if (notification.Status != "Failed")
        {
            return BadRequest("Can only retry failed notifications");
        }

        try
        {
            switch (notification.Type.ToLower())
            {
                case "email":
                    await _emailService.SendEmailAsync(
                        notification.Recipient,
                        notification.Subject,
                        notification.Body);
                    break;
                default:
                    throw new NotSupportedException($"Notification type '{notification.Type}' is not supported");
            }

            notification.Status = "Sent";
            notification.SentAt = DateTime.UtcNow;
            notification.ErrorMessage = null;
            notification.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Notification {NotificationId} retry successful", notification.Id);

            return NoContent();
        }
        catch (Exception ex)
        {
            notification.ErrorMessage = ex.Message;
            notification.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogError(ex, "Failed to retry notification {NotificationId}", notification.Id);
            return BadRequest("Retry failed: " + ex.Message);
        }
    }

    [HttpGet("statistics")]
    [Authorize(Roles = "Admin,NotificationManager")]
    public async Task<ActionResult<NotificationStatisticsDto>> GetStatistics(
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
        var from = fromDate ?? DateTime.UtcNow.AddDays(-30);
        var to = toDate ?? DateTime.UtcNow;

        var query = _context.Notifications
            .Where(n => n.CreatedAt >= from && n.CreatedAt <= to);

        var statistics = new NotificationStatisticsDto
        {
            TotalNotifications = await query.CountAsync(),
            SentNotifications = await query.CountAsync(n => n.Status == "Sent"),
            FailedNotifications = await query.CountAsync(n => n.Status == "Failed"),
            PendingNotifications = await query.CountAsync(n => n.Status == "Pending"),
            NotificationsByType = await query
                .GroupBy(n => n.Type)
                .Select(g => new NotificationTypeStatDto
                {
                    Type = g.Key,
                    Count = g.Count(),
                    SentCount = g.Count(n => n.Status == "Sent"),
                    FailedCount = g.Count(n => n.Status == "Failed")
                })
                .ToListAsync(),
            FromDate = from,
            ToDate = to
        };

        return Ok(statistics);
    }
}

// DTOs
public class NotificationDto
{
    public Guid Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Recipient { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime? SentAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class NotificationDetailDto : NotificationDto
{
    public string Body { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
    public string Metadata { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; }
}

public class NotificationStatisticsDto
{
    public int TotalNotifications { get; set; }
    public int SentNotifications { get; set; }
    public int FailedNotifications { get; set; }
    public int PendingNotifications { get; set; }
    public List<NotificationTypeStatDto> NotificationsByType { get; set; } = new();
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
}

public class NotificationTypeStatDto
{
    public string Type { get; set; } = string.Empty;
    public int Count { get; set; }
    public int SentCount { get; set; }
    public int FailedCount { get; set; }
}

public class BulkNotificationResponseDto
{
    public int TotalSent { get; set; }
    public int TotalFailed { get; set; }
    public List<NotificationDto> Results { get; set; } = new();
}

// Request models
public class SendNotificationRequest
{
    public string Type { get; set; } = string.Empty;
    public string Recipient { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string? TemplateId { get; set; }
    public string? Metadata { get; set; }
}

public class BulkNotificationRequest
{
    public string Type { get; set; } = string.Empty;
    public List<string> Recipients { get; set; } = new();
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string? TemplateId { get; set; }
    public string? Metadata { get; set; }
}

// Events
public class NotificationSentEvent : IDomainEvent
{
    public Guid NotificationId { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Recipient { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime? SentAt { get; set; }
    public string? ErrorMessage { get; set; }
}

// Validators
public class SendNotificationRequestValidator : AbstractValidator<SendNotificationRequest>
{
    public SendNotificationRequestValidator()
    {
        RuleFor(x => x.Type)
            .NotEmpty()
            .Must(BeValidType)
            .WithMessage("Type must be one of: Email, SMS, Push, InApp");

        RuleFor(x => x.Recipient)
            .NotEmpty()
            .EmailAddress()
            .When(x => x.Type.ToLower() == "email");

        RuleFor(x => x.Subject)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Body)
            .NotEmpty()
            .MaximumLength(10000);

        RuleFor(x => x.Metadata)
            .Must(BeValidJsonOrNull)
            .WithMessage("Metadata must be valid JSON");
    }

    private bool BeValidType(string type)
    {
        var validTypes = new[] { "email", "sms", "push", "inapp" };
        return validTypes.Contains(type.ToLower());
    }

    private bool BeValidJsonOrNull(string? metadata)
    {
        if (string.IsNullOrEmpty(metadata))
            return true;

        try
        {
            System.Text.Json.JsonDocument.Parse(metadata);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
