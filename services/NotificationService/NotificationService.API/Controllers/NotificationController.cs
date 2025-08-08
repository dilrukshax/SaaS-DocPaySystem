using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace NotificationService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NotificationController : ControllerBase
{
    private readonly ILogger<NotificationController> _logger;

    public NotificationController(ILogger<NotificationController> logger)
    {
        _logger = logger;
    }

    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok(new { Status = "Healthy", Service = "NotificationService", Timestamp = DateTime.UtcNow });
    }

    [HttpPost("send")]
    [Authorize]
    public IActionResult SendNotification([FromBody] object notificationData)
    {
        // Placeholder implementation
        _logger.LogInformation("Sending notification");
        return Ok(new { Message = "Notification sent successfully", NotificationId = Guid.NewGuid() });
    }

    [HttpGet]
    [Authorize]
    public IActionResult GetNotifications()
    {
        // Placeholder implementation
        var notifications = new[]
        {
            new { Id = 1, Type = "Email", Status = "Sent", Recipient = "user@example.com", Date = DateTime.UtcNow.AddMinutes(-30) },
            new { Id = 2, Type = "SMS", Status = "Pending", Recipient = "+1234567890", Date = DateTime.UtcNow }
        };

        return Ok(notifications);
    }

    [HttpGet("{id}")]
    [Authorize]
    public IActionResult GetNotification(int id)
    {
        // Placeholder implementation
        var notification = new { Id = id, Type = "Email", Status = "Sent", Recipient = "user@example.com", Date = DateTime.UtcNow };
        return Ok(notification);
    }

    [HttpPost("templates")]
    [Authorize]
    public IActionResult CreateTemplate([FromBody] object templateData)
    {
        // Placeholder implementation
        _logger.LogInformation("Creating notification template");
        return Ok(new { Message = "Template created successfully", TemplateId = Guid.NewGuid() });
    }
}
