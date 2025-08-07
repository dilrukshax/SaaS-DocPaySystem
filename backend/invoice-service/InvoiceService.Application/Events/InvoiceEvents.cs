namespace InvoiceService.Application.Events;

// Invoice Created Event
public record InvoiceCreatedEvent(
    Guid InvoiceId,
    string InvoiceNumber,
    Guid DocumentId,
    Guid CustomerId,
    decimal Amount,
    string Currency,
    DateTime DueDate,
    Guid TenantId,
    Guid CreatedBy,
    DateTime CreatedAt);

// Invoice Updated Event
public record InvoiceUpdatedEvent(
    Guid InvoiceId,
    string InvoiceNumber,
    decimal Amount,
    DateTime DueDate,
    Guid TenantId,
    Guid UpdatedBy,
    DateTime UpdatedAt);

// Invoice Deleted Event
public record InvoiceDeletedEvent(
    Guid InvoiceId,
    string InvoiceNumber,
    Guid TenantId,
    Guid DeletedBy,
    DateTime DeletedAt);

// Invoice Sent Event
public record InvoiceSentEvent(
    Guid InvoiceId,
    string InvoiceNumber,
    string RecipientEmail,
    Guid TenantId,
    Guid SentBy,
    DateTime SentAt);

// Payment Processed Event
public record PaymentProcessedEvent(
    Guid InvoiceId,
    Guid PaymentId,
    decimal Amount,
    string PaymentMethod,
    string Status,
    Guid TenantId,
    Guid ProcessedBy,
    DateTime ProcessedAt);

// Invoice Status Changed Event
public record InvoiceStatusChangedEvent(
    Guid InvoiceId,
    string InvoiceNumber,
    string OldStatus,
    string NewStatus,
    Guid TenantId,
    Guid UpdatedBy,
    DateTime ChangedAt);

// Invoice Overdue Event
public record InvoiceOverdueEvent(
    Guid InvoiceId,
    string InvoiceNumber,
    Guid CustomerId,
    decimal OutstandingAmount,
    DateTime DueDate,
    int DaysOverdue,
    Guid TenantId,
    DateTime DetectedAt);

// Invoice Paid Event
public record InvoicePaidEvent(
    Guid InvoiceId,
    string InvoiceNumber,
    Guid CustomerId,
    decimal TotalAmount,
    decimal PaidAmount,
    Guid TenantId,
    DateTime PaidAt);
