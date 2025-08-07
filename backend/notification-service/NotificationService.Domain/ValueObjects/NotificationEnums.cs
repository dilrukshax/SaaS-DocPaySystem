namespace NotificationService.Domain.ValueObjects;

public enum NotificationType
{
    System = 0,
    DocumentApproval = 1,
    InvoiceApproval = 2,
    PaymentAlert = 3,
    WorkflowTask = 4,
    SecurityAlert = 5,
    Welcome = 6,
    PasswordReset = 7,
    Reminder = 8
}

public enum NotificationPriority
{
    Low = 0,
    Normal = 1,
    High = 2,
    Critical = 3
}

public enum NotificationStatus
{
    Pending = 0,
    Scheduled = 1,
    Sent = 2,
    Delivered = 3,
    Failed = 4,
    Cancelled = 5
}

public enum ChannelType
{
    Email = 0,
    SMS = 1,
    Push = 2,
    InApp = 3,
    Slack = 4,
    Teams = 5
}

public enum ChannelStatus
{
    Pending = 0,
    Sent = 1,
    Delivered = 2,
    Failed = 3,
    Bounced = 4
}
