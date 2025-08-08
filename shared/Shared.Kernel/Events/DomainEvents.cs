namespace Shared.Kernel.Events;

// Base Event Types
public abstract record DomainEvent(
    Guid AggregateId,
    Guid TenantId,
    DateTime OccurredOn);

// Document Events
public record DocumentUploadedEvent(
    Guid DocumentId,
    string Name,
    string MimeType,
    long FileSize,
    Guid TenantId,
    Guid UserId,
    DateTime OccurredOn) : DomainEvent(DocumentId, TenantId, OccurredOn);

public record DocumentDeletedEvent(
    Guid DocumentId,
    string Name,
    Guid TenantId,
    Guid DeletedBy,
    DateTime OccurredOn) : DomainEvent(DocumentId, TenantId, OccurredOn);

public record DocumentOCRProcessedEvent(
    Guid DocumentId,
    string ExtractedText,
    double Confidence,
    Guid TenantId,
    DateTime OccurredOn) : DomainEvent(DocumentId, TenantId, OccurredOn);

// Invoice Events
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
    DateTime OccurredOn) : DomainEvent(InvoiceId, TenantId, OccurredOn);

public record InvoiceStatusChangedEvent(
    Guid InvoiceId,
    string InvoiceNumber,
    string OldStatus,
    string NewStatus,
    Guid TenantId,
    Guid UpdatedBy,
    DateTime OccurredOn) : DomainEvent(InvoiceId, TenantId, OccurredOn);

public record InvoicePaidEvent(
    Guid InvoiceId,
    string InvoiceNumber,
    Guid CustomerId,
    decimal TotalAmount,
    decimal PaidAmount,
    Guid TenantId,
    DateTime OccurredOn) : DomainEvent(InvoiceId, TenantId, OccurredOn);

// Payment Events
public record PaymentInitiatedEvent(
    Guid PaymentId,
    Guid InvoiceId,
    decimal Amount,
    string Currency,
    string PaymentMethod,
    Guid TenantId,
    Guid InitiatedBy,
    DateTime OccurredOn) : DomainEvent(PaymentId, TenantId, OccurredOn);

public record PaymentProcessedEvent(
    Guid PaymentId,
    Guid InvoiceId,
    decimal Amount,
    string Status,
    string? PaymentReference,
    Guid TenantId,
    DateTime OccurredOn) : DomainEvent(PaymentId, TenantId, OccurredOn);

public record PaymentFailedEvent(
    Guid PaymentId,
    Guid InvoiceId,
    decimal Amount,
    string ErrorMessage,
    Guid TenantId,
    DateTime OccurredOn) : DomainEvent(PaymentId, TenantId, OccurredOn);

// Workflow Events
public record WorkflowStartedEvent(
    Guid WorkflowInstanceId,
    Guid WorkflowDefinitionId,
    string WorkflowType,
    Guid TriggeredBy,
    Dictionary<string, object> InitialData,
    Guid TenantId,
    DateTime OccurredOn) : DomainEvent(WorkflowInstanceId, TenantId, OccurredOn);

public record WorkflowStepCompletedEvent(
    Guid WorkflowInstanceId,
    Guid StepId,
    string StepName,
    string Status,
    Guid? AssignedTo,
    Guid? CompletedBy,
    Dictionary<string, object>? OutputData,
    Guid TenantId,
    DateTime OccurredOn) : DomainEvent(WorkflowInstanceId, TenantId, OccurredOn);

public record WorkflowCompletedEvent(
    Guid WorkflowInstanceId,
    string FinalStatus,
    Dictionary<string, object> FinalData,
    Guid TenantId,
    DateTime OccurredOn) : DomainEvent(WorkflowInstanceId, TenantId, OccurredOn);

// User Events
public record UserRegisteredEvent(
    Guid UserId,
    string Email,
    string FirstName,
    string LastName,
    Guid TenantId,
    DateTime OccurredOn) : DomainEvent(UserId, TenantId, OccurredOn);

public record UserRoleAssignedEvent(
    Guid UserId,
    string RoleName,
    Guid AssignedBy,
    Guid TenantId,
    DateTime OccurredOn) : DomainEvent(UserId, TenantId, OccurredOn);

// Notification Events
public record NotificationCreatedEvent(
    Guid NotificationId,
    Guid RecipientId,
    string Type,
    string Title,
    string Message,
    Guid TenantId,
    DateTime OccurredOn) : DomainEvent(NotificationId, TenantId, OccurredOn);

public record NotificationSentEvent(
    Guid NotificationId,
    Guid RecipientId,
    string Channel,
    string Status,
    Guid TenantId,
    DateTime OccurredOn) : DomainEvent(NotificationId, TenantId, OccurredOn);
