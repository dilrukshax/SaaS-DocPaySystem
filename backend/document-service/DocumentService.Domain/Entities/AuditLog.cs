namespace DocumentService.Domain.Entities;

public class AuditLog
{
    public Guid Id { get; private set; }
    public Guid DocumentId { get; private set; }
    public string Action { get; private set; }
    public Guid UserId { get; private set; }
    public DateTime Timestamp { get; private set; }
    public string? Details { get; private set; }
    public string? IpAddress { get; private set; }
    public string? UserAgent { get; private set; }

    // Navigation property
    public Document Document { get; private set; } = null!;

    private AuditLog() { } // For EF Core

    public AuditLog(Guid documentId, string action, Guid userId, string? details = null)
    {
        Id = Guid.NewGuid();
        DocumentId = documentId;
        Action = action ?? throw new ArgumentNullException(nameof(action));
        UserId = userId;
        Timestamp = DateTime.UtcNow;
        Details = details;
    }

    public void SetContextInfo(string? ipAddress, string? userAgent)
    {
        IpAddress = ipAddress;
        UserAgent = userAgent;
    }
}
