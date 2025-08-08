using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NotificationService.Domain.Entities;
using NotificationService.Infrastructure.Data;
using FluentValidation;

namespace NotificationService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,NotificationManager")]
public class NotificationTemplatesController : ControllerBase
{
    private readonly NotificationDbContext _context;
    private readonly IValidator<CreateTemplateRequest> _createValidator;
    private readonly IValidator<UpdateTemplateRequest> _updateValidator;
    private readonly ILogger<NotificationTemplatesController> _logger;

    public NotificationTemplatesController(
        NotificationDbContext context,
        IValidator<CreateTemplateRequest> createValidator,
        IValidator<UpdateTemplateRequest> updateValidator,
        ILogger<NotificationTemplatesController> logger)
    {
        _context = context;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<NotificationTemplateDto>>> GetTemplates(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? type = null,
        [FromQuery] string? search = null)
    {
        var query = _context.NotificationTemplates.AsQueryable();

        if (!string.IsNullOrEmpty(type))
        {
            query = query.Where(t => t.Type == type);
        }

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(t => t.Name.Contains(search) || t.Subject.Contains(search));
        }

        var totalCount = await query.CountAsync();
        var templates = await query
            .OrderBy(t => t.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new NotificationTemplateDto
            {
                Id = t.Id,
                Name = t.Name,
                Type = t.Type,
                Subject = t.Subject,
                IsActive = t.IsActive,
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt
            })
            .ToListAsync();

        Response.Headers.Add("X-Total-Count", totalCount.ToString());
        return Ok(templates);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<NotificationTemplateDetailDto>> GetTemplate(Guid id)
    {
        var template = await _context.NotificationTemplates.FindAsync(id);
        if (template == null)
        {
            return NotFound();
        }

        var dto = new NotificationTemplateDetailDto
        {
            Id = template.Id,
            Name = template.Name,
            Type = template.Type,
            Subject = template.Subject,
            Body = template.Body,
            IsActive = template.IsActive,
            Variables = template.Variables,
            CreatedAt = template.CreatedAt,
            UpdatedAt = template.UpdatedAt
        };

        return Ok(dto);
    }

    [HttpPost]
    public async Task<ActionResult<NotificationTemplateDto>> CreateTemplate(CreateTemplateRequest request)
    {
        var validationResult = await _createValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.Errors);
        }

        // Check if template name already exists
        if (await _context.NotificationTemplates.AnyAsync(t => t.Name == request.Name))
        {
            return BadRequest("Template name already exists");
        }

        var template = new NotificationTemplate
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Type = request.Type,
            Subject = request.Subject,
            Body = request.Body,
            IsActive = true,
            Variables = request.Variables ?? "[]",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.NotificationTemplates.Add(template);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Notification template created: {TemplateId} by user {UserId}", 
            template.Id, User.Identity?.Name);

        var dto = new NotificationTemplateDto
        {
            Id = template.Id,
            Name = template.Name,
            Type = template.Type,
            Subject = template.Subject,
            IsActive = template.IsActive,
            CreatedAt = template.CreatedAt,
            UpdatedAt = template.UpdatedAt
        };

        return CreatedAtAction(nameof(GetTemplate), new { id = template.Id }, dto);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTemplate(Guid id, UpdateTemplateRequest request)
    {
        var validationResult = await _updateValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.Errors);
        }

        var template = await _context.NotificationTemplates.FindAsync(id);
        if (template == null)
        {
            return NotFound();
        }

        // Check if template name already exists (excluding current template)
        if (await _context.NotificationTemplates.AnyAsync(t => t.Name == request.Name && t.Id != id))
        {
            return BadRequest("Template name already exists");
        }

        template.Name = request.Name;
        template.Type = request.Type;
        template.Subject = request.Subject;
        template.Body = request.Body;
        template.Variables = request.Variables ?? "[]";
        template.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Notification template updated: {TemplateId} by user {UserId}", 
            template.Id, User.Identity?.Name);

        return NoContent();
    }

    [HttpPost("{id}/activate")]
    public async Task<IActionResult> ActivateTemplate(Guid id)
    {
        var template = await _context.NotificationTemplates.FindAsync(id);
        if (template == null)
        {
            return NotFound();
        }

        template.IsActive = true;
        template.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Notification template activated: {TemplateId} by user {UserId}", 
            template.Id, User.Identity?.Name);

        return NoContent();
    }

    [HttpPost("{id}/deactivate")]
    public async Task<IActionResult> DeactivateTemplate(Guid id)
    {
        var template = await _context.NotificationTemplates.FindAsync(id);
        if (template == null)
        {
            return NotFound();
        }

        template.IsActive = false;
        template.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Notification template deactivated: {TemplateId} by user {UserId}", 
            template.Id, User.Identity?.Name);

        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteTemplate(Guid id)
    {
        var template = await _context.NotificationTemplates.FindAsync(id);
        if (template == null)
        {
            return NotFound();
        }

        _context.NotificationTemplates.Remove(template);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Notification template deleted: {TemplateId} by user {UserId}", 
            template.Id, User.Identity?.Name);

        return NoContent();
    }

    [HttpPost("{id}/preview")]
    public async Task<ActionResult<TemplatePreviewDto>> PreviewTemplate(Guid id, [FromBody] PreviewTemplateRequest request)
    {
        var template = await _context.NotificationTemplates.FindAsync(id);
        if (template == null)
        {
            return NotFound();
        }

        try
        {
            var renderedSubject = RenderTemplate(template.Subject, request.Variables);
            var renderedBody = RenderTemplate(template.Body, request.Variables);

            var preview = new TemplatePreviewDto
            {
                TemplateId = template.Id,
                TemplateName = template.Name,
                RenderedSubject = renderedSubject,
                RenderedBody = renderedBody,
                Variables = request.Variables
            };

            return Ok(preview);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to preview template {TemplateId}", id);
            return BadRequest("Template rendering failed: " + ex.Message);
        }
    }

    private string RenderTemplate(string template, Dictionary<string, object> variables)
    {
        var result = template;
        foreach (var variable in variables)
        {
            var placeholder = $"{{{{{variable.Key}}}}}";
            result = result.Replace(placeholder, variable.Value?.ToString() ?? string.Empty);
        }
        return result;
    }
}

// DTOs
public class NotificationTemplateDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class NotificationTemplateDetailDto : NotificationTemplateDto
{
    public string Body { get; set; } = string.Empty;
    public string Variables { get; set; } = string.Empty;
}

public class TemplatePreviewDto
{
    public Guid TemplateId { get; set; }
    public string TemplateName { get; set; } = string.Empty;
    public string RenderedSubject { get; set; } = string.Empty;
    public string RenderedBody { get; set; } = string.Empty;
    public Dictionary<string, object> Variables { get; set; } = new();
}

// Request models
public class CreateTemplateRequest
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string? Variables { get; set; }
}

public class UpdateTemplateRequest
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string? Variables { get; set; }
}

public class PreviewTemplateRequest
{
    public Dictionary<string, object> Variables { get; set; } = new();
}

// Validators
public class CreateTemplateRequestValidator : AbstractValidator<CreateTemplateRequest>
{
    public CreateTemplateRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Type)
            .NotEmpty()
            .Must(BeValidType)
            .WithMessage("Type must be one of: Email, SMS, Push, InApp");

        RuleFor(x => x.Subject)
            .NotEmpty()
            .MaximumLength(500);

        RuleFor(x => x.Body)
            .NotEmpty()
            .MaximumLength(50000);

        RuleFor(x => x.Variables)
            .Must(BeValidJsonOrNull)
            .WithMessage("Variables must be valid JSON array");
    }

    private bool BeValidType(string type)
    {
        var validTypes = new[] { "email", "sms", "push", "inapp" };
        return validTypes.Contains(type.ToLower());
    }

    private bool BeValidJsonOrNull(string? variables)
    {
        if (string.IsNullOrEmpty(variables))
            return true;

        try
        {
            System.Text.Json.JsonDocument.Parse(variables);
            return true;
        }
        catch
        {
            return false;
        }
    }
}

public class UpdateTemplateRequestValidator : AbstractValidator<UpdateTemplateRequest>
{
    public UpdateTemplateRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Type)
            .NotEmpty()
            .Must(BeValidType)
            .WithMessage("Type must be one of: Email, SMS, Push, InApp");

        RuleFor(x => x.Subject)
            .NotEmpty()
            .MaximumLength(500);

        RuleFor(x => x.Body)
            .NotEmpty()
            .MaximumLength(50000);

        RuleFor(x => x.Variables)
            .Must(BeValidJsonOrNull)
            .WithMessage("Variables must be valid JSON array");
    }

    private bool BeValidType(string type)
    {
        var validTypes = new[] { "email", "sms", "push", "inapp" };
        return validTypes.Contains(type.ToLower());
    }

    private bool BeValidJsonOrNull(string? variables)
    {
        if (string.IsNullOrEmpty(variables))
            return true;

        try
        {
            System.Text.Json.JsonDocument.Parse(variables);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
