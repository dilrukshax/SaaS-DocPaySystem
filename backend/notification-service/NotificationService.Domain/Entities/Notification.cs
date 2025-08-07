using NotificationService.Domain.ValueObjects;

namespace NotificationService.Domain.Entities;

public class Notification
{
    public Guid Id { get; private set; }
    public string Subject { get; private set; }
    public string Content { get; private set; }
    public NotificationType Type { get; private set; }
    public NotificationPriority Priority { get; private set; }
    public Guid RecipientId { get; private set; }
    public string? RecipientEmail { get; private set; }
    public string? RecipientPhone { get; private set; }
    public NotificationStatus Status { get; private set; }
    public Guid TenantId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? ScheduledAt { get; private set; }
    public DateTime? SentAt { get; private set; }
    public string? ErrorMessage { get; private set; }
    public int RetryCount { get; private set; }
    public string? Metadata { get; private set; }

    private readonly List<Channel> _channels = new();
    public IReadOnlyList<Channel> Channels => _channels.AsReadOnly();

    private Notification() { } // For EF Core

    public Notification(string subject, string content, NotificationType type, Guid recipientId, 
        Guid tenantId, NotificationPriority priority = NotificationPriority.Normal, 
        string? recipientEmail = null, string? recipientPhone = null, DateTime? scheduledAt = null)
    {
        Id = Guid.NewGuid();
        Subject = subject ?? throw new ArgumentNullException(nameof(subject));
        Content = content ?? throw new ArgumentNullException(nameof(content));
        Type = type;
        Priority = priority;
        RecipientId = recipientId;
        RecipientEmail = recipientEmail;
        RecipientPhone = recipientPhone;
        TenantId = tenantId;
        Status = scheduledAt.HasValue ? NotificationStatus.Scheduled : NotificationStatus.Pending;
        ScheduledAt = scheduledAt;
        CreatedAt = DateTime.UtcNow;
        RetryCount = 0;
    }

    public void AddChannel(ChannelType channelType, string? configuration = null)
    {
        var channel = new Channel(Id, channelType, configuration);
        _channels.Add(channel);
    }

    public void MarkAsSent()
    {
        Status = NotificationStatus.Sent;
        SentAt = DateTime.UtcNow;
    }

    public void MarkAsFailed(string errorMessage)
    {
        Status = NotificationStatus.Failed;
        ErrorMessage = errorMessage;
        RetryCount++;
    }

    public void MarkAsDelivered()
    {
        Status = NotificationStatus.Delivered;
    }

    public void Schedule(DateTime scheduledAt)
    {
        if (Status != NotificationStatus.Pending)
            throw new InvalidOperationException("Can only schedule pending notifications");

        ScheduledAt = scheduledAt;
        Status = NotificationStatus.Scheduled;
    }

    public void Cancel()
    {
        if (Status == NotificationStatus.Sent || Status == NotificationStatus.Delivered)
            throw new InvalidOperationException("Cannot cancel sent or delivered notifications");

        Status = NotificationStatus.Cancelled;
    }

    public void SetMetadata(string metadata)
    {
        Metadata = metadata;
    }

    public bool CanRetry()
    {
        return Status == NotificationStatus.Failed && RetryCount < 3;
    }
}
