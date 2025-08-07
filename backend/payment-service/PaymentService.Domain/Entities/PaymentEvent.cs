namespace PaymentService.Domain.Entities;

public class PaymentEvent
{
    public Guid Id { get; private set; }
    public Guid PaymentId { get; private set; }
    public string EventType { get; private set; }
    public string Description { get; private set; }
    public DateTime Timestamp { get; private set; }
    public string? Metadata { get; private set; }

    // Navigation property
    public Payment Payment { get; private set; } = null!;

    private PaymentEvent() { } // For EF Core

    public PaymentEvent(Guid paymentId, string eventType, string description, string? metadata = null)
    {
        Id = Guid.NewGuid();
        PaymentId = paymentId;
        EventType = eventType ?? throw new ArgumentNullException(nameof(eventType));
        Description = description ?? throw new ArgumentNullException(nameof(description));
        Timestamp = DateTime.UtcNow;
        Metadata = metadata;
    }
}
