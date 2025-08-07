namespace WorkflowService.Domain.ValueObjects;

public enum WorkflowStatus
{
    Running = 0,
    Completed = 1,
    Cancelled = 2,
    Failed = 3,
    Suspended = 4
}

public enum TaskStatus
{
    Pending = 0,
    InProgress = 1,
    Completed = 2,
    Rejected = 3,
    Cancelled = 4,
    Overdue = 5
}

public enum TaskType
{
    Approval = 0,
    Review = 1,
    DataEntry = 2,
    Notification = 3,
    SystemAction = 4,
    Manual = 5
}
