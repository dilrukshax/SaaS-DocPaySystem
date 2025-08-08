namespace Shared.Kernel.Constants;

public static class Roles
{
    public const string Admin = "Admin";
    public const string Manager = "Manager";
    public const string Approver = "Approver";
    public const string Viewer = "Viewer";
    public const string User = "User";

    public static readonly string[] AllRoles = { Admin, Manager, Approver, Viewer, User };
}

public static class Permissions
{
    // Document permissions
    public const string DocumentCreate = "document:create";
    public const string DocumentRead = "document:read";
    public const string DocumentUpdate = "document:update";
    public const string DocumentDelete = "document:delete";
    public const string DocumentProcess = "document:process";

    // Invoice permissions
    public const string InvoiceCreate = "invoice:create";
    public const string InvoiceRead = "invoice:read";
    public const string InvoiceUpdate = "invoice:update";
    public const string InvoiceDelete = "invoice:delete";
    public const string InvoiceSend = "invoice:send";

    // Payment permissions
    public const string PaymentProcess = "payment:process";
    public const string PaymentRead = "payment:read";
    public const string PaymentRefund = "payment:refund";

    // Workflow permissions
    public const string WorkflowCreate = "workflow:create";
    public const string WorkflowRead = "workflow:read";
    public const string WorkflowApprove = "workflow:approve";
    public const string WorkflowReject = "workflow:reject";

    // User permissions
    public const string UserCreate = "user:create";
    public const string UserRead = "user:read";
    public const string UserUpdate = "user:update";
    public const string UserDelete = "user:delete";
    public const string UserManageRoles = "user:manage_roles";

    // Report permissions
    public const string ReportView = "report:view";
    public const string ReportGenerate = "report:generate";
    public const string ReportExport = "report:export";
}

public static class EventTypes
{
    // Document events
    public const string DocumentUploaded = "document.uploaded";
    public const string DocumentDeleted = "document.deleted";
    public const string DocumentOCRProcessed = "document.ocr_processed";

    // Invoice events
    public const string InvoiceCreated = "invoice.created";
    public const string InvoiceStatusChanged = "invoice.status_changed";
    public const string InvoicePaid = "invoice.paid";

    // Payment events
    public const string PaymentInitiated = "payment.initiated";
    public const string PaymentProcessed = "payment.processed";
    public const string PaymentFailed = "payment.failed";

    // Workflow events
    public const string WorkflowStarted = "workflow.started";
    public const string WorkflowStepCompleted = "workflow.step_completed";
    public const string WorkflowCompleted = "workflow.completed";

    // User events
    public const string UserRegistered = "user.registered";
    public const string UserRoleAssigned = "user.role_assigned";

    // Notification events
    public const string NotificationCreated = "notification.created";
    public const string NotificationSent = "notification.sent";
}

public static class NotificationChannels
{
    public const string Email = "email";
    public const string SMS = "sms";
    public const string InApp = "in_app";
    public const string Push = "push";
}

public static class WorkflowTypes
{
    public const string DocumentApproval = "document_approval";
    public const string InvoiceApproval = "invoice_approval";
    public const string PaymentApproval = "payment_approval";
    public const string ExpenseApproval = "expense_approval";
}

public static class PaymentMethods
{
    public const string CreditCard = "credit_card";
    public const string BankTransfer = "bank_transfer";
    public const string PayPal = "paypal";
    public const string Stripe = "stripe";
    public const string Check = "check";
    public const string Cash = "cash";
}

public static class DocumentStatuses
{
    public const string Draft = "draft";
    public const string Processing = "processing";
    public const string Processed = "processed";
    public const string Approved = "approved";
    public const string Rejected = "rejected";
    public const string Archived = "archived";
}

public static class InvoiceStatuses
{
    public const string Draft = "draft";
    public const string Sent = "sent";
    public const string Viewed = "viewed";
    public const string PartiallyPaid = "partially_paid";
    public const string Paid = "paid";
    public const string Overdue = "overdue";
    public const string Cancelled = "cancelled";
}

public static class PaymentStatuses
{
    public const string Pending = "pending";
    public const string Processing = "processing";
    public const string Completed = "completed";
    public const string Failed = "failed";
    public const string Cancelled = "cancelled";
    public const string Refunded = "refunded";
}
