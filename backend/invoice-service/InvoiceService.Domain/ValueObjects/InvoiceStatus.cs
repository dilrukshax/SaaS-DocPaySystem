namespace InvoiceService.Domain.ValueObjects;

public enum InvoiceStatus
{
    Draft = 0,
    PendingApproval = 1,
    Approved = 2,
    Rejected = 3,
    Paid = 4,
    Overdue = 5,
    Cancelled = 6,
    PartiallyPaid = 7
}
