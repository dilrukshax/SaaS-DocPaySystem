using PaymentService.Domain.ValueObjects;

namespace PaymentService.Domain.Entities;

public class Payment
{
    public Guid Id { get; private set; }
    public string PaymentReference { get; private set; }
    public Guid? InvoiceId { get; private set; }
    public decimal Amount { get; private set; }
    public string Currency { get; private set; }
    public PaymentStatus Status { get; private set; }
    public Guid PaymentMethodId { get; private set; }
    public string? ExternalTransactionId { get; private set; }
    public string? GatewayResponse { get; private set; }
    public DateTime? ProcessedAt { get; private set; }
    public string? FailureReason { get; private set; }
    public Guid TenantId { get; private set; }
    public Guid InitiatedBy { get; private set; }
    public DateTime InitiatedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public string? Notes { get; private set; }
    public bool IsRefunded { get; private set; }
    public decimal RefundedAmount { get; private set; }

    // Navigation property
    public PaymentMethod PaymentMethod { get; private set; } = null!;

    private readonly List<PaymentEvent> _events = new();
    public IReadOnlyList<PaymentEvent> Events => _events.AsReadOnly();

    private Payment() { } // For EF Core

    public Payment(string paymentReference, decimal amount, string currency, 
        Guid paymentMethodId, Guid tenantId, Guid initiatedBy, Guid? invoiceId = null, string? notes = null)
    {
        Id = Guid.NewGuid();
        PaymentReference = paymentReference ?? throw new ArgumentNullException(nameof(paymentReference));
        InvoiceId = invoiceId;
        Amount = amount;
        Currency = currency ?? throw new ArgumentNullException(nameof(currency));
        Status = PaymentStatus.Pending;
        PaymentMethodId = paymentMethodId;
        TenantId = tenantId;
        InitiatedBy = initiatedBy;
        InitiatedAt = DateTime.UtcNow;
        Notes = notes;
        IsRefunded = false;
        RefundedAmount = 0;

        AddEvent("Payment initiated", $"Payment of {amount} {currency} initiated");
    }

    public void MarkAsProcessing(string? externalTransactionId = null)
    {
        if (Status != PaymentStatus.Pending)
            throw new InvalidOperationException("Can only mark pending payments as processing");

        Status = PaymentStatus.Processing;
        ExternalTransactionId = externalTransactionId;
        ProcessedAt = DateTime.UtcNow;

        AddEvent("Payment processing", "Payment is being processed by gateway");
    }

    public void MarkAsCompleted(string? gatewayResponse = null)
    {
        if (Status != PaymentStatus.Processing)
            throw new InvalidOperationException("Can only complete processing payments");

        Status = PaymentStatus.Completed;
        GatewayResponse = gatewayResponse;
        CompletedAt = DateTime.UtcNow;

        AddEvent("Payment completed", "Payment completed successfully");
    }

    public void MarkAsFailed(string failureReason, string? gatewayResponse = null)
    {
        if (Status == PaymentStatus.Completed || Status == PaymentStatus.Refunded)
            throw new InvalidOperationException("Cannot fail completed or refunded payments");

        Status = PaymentStatus.Failed;
        FailureReason = failureReason;
        GatewayResponse = gatewayResponse;

        AddEvent("Payment failed", failureReason);
    }

    public void ProcessRefund(decimal refundAmount, string reason, Guid processedBy)
    {
        if (Status != PaymentStatus.Completed)
            throw new InvalidOperationException("Can only refund completed payments");

        if (refundAmount <= 0 || refundAmount > (Amount - RefundedAmount))
            throw new InvalidOperationException("Invalid refund amount");

        RefundedAmount += refundAmount;
        
        if (RefundedAmount >= Amount)
        {
            Status = PaymentStatus.Refunded;
            IsRefunded = true;
        }
        else
        {
            Status = PaymentStatus.PartiallyRefunded;
        }

        AddEvent("Payment refunded", $"Refund of {refundAmount} {Currency} processed. Reason: {reason}");
    }

    private void AddEvent(string eventType, string description)
    {
        var paymentEvent = new PaymentEvent(Id, eventType, description);
        _events.Add(paymentEvent);
    }
}
