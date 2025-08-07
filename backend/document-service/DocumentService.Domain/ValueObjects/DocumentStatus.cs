namespace DocumentService.Domain.ValueObjects;

public enum DocumentStatus
{
    Processing = 0,
    Available = 1,
    Archived = 2,
    Error = 3,
    PendingApproval = 4,
    Approved = 5,
    Rejected = 6
}
