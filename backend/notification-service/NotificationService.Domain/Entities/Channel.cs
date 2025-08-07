using NotificationService.Domain.ValueObjects;

namespace NotificationService.Domain.Entities;

public class Channel
{
    public Guid Id { get; private set; }
    public Guid NotificationId { get; private set; }
    public ChannelType Type { get; private set; }
    public ChannelStatus Status { get; private set; }
    public string? Configuration { get; private set; }
    public DateTime? SentAt { get; private set; }
    public DateTime? DeliveredAt { get; private set; }
    public string? ErrorMessage { get; private set; }
    public string? ExternalId { get; private set; }

    // Navigation property
    public Notification Notification { get; private set; } = null!;

    private Channel() { } // For EF Core

    public Channel(Guid notificationId, ChannelType type, string? configuration = null)
    {
        Id = Guid.NewGuid();
        NotificationId = notificationId;
        Type = type;
        Configuration = configuration;
        Status = ChannelStatus.Pending;
    }

    public void MarkAsSent(string? externalId = null)
    {
        Status = ChannelStatus.Sent;
        SentAt = DateTime.UtcNow;
        ExternalId = externalId;
    }

    public void MarkAsDelivered()
    {
        Status = ChannelStatus.Delivered;
        DeliveredAt = DateTime.UtcNow;
    }

    public void MarkAsFailed(string errorMessage)
    {
        Status = ChannelStatus.Failed;
        ErrorMessage = errorMessage;
    }
}
