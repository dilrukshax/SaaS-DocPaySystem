using WorkflowService.Domain.ValueObjects;

namespace WorkflowService.Domain.Entities;

public class Task
{
    public Guid Id { get; private set; }
    public Guid WorkflowInstanceId { get; private set; }
    public string Title { get; private set; }
    public string Description { get; private set; }
    public TaskType Type { get; private set; }
    public TaskStatus Status { get; private set; }
    public Guid AssignedTo { get; private set; }
    public Guid? AssignedBy { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? DueDate { get; private set; }
    public DateTime? StartedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public string? CompletionNotes { get; private set; }
    public string? Outcome { get; private set; }
    public int Priority { get; private set; }
    public string? FormData { get; private set; }

    // Navigation property
    public WorkflowInstance WorkflowInstance { get; private set; } = null!;

    private Task() { } // For EF Core

    public Task(Guid workflowInstanceId, string title, string description, Guid assignedTo, 
        TaskType type, int priority = 1, DateTime? dueDate = null)
    {
        Id = Guid.NewGuid();
        WorkflowInstanceId = workflowInstanceId;
        Title = title ?? throw new ArgumentNullException(nameof(title));
        Description = description ?? string.Empty;
        Type = type;
        Status = TaskStatus.Pending;
        AssignedTo = assignedTo;
        CreatedAt = DateTime.UtcNow;
        DueDate = dueDate;
        Priority = Math.Max(1, Math.Min(5, priority));
    }

    public void Start(Guid? startedBy = null)
    {
        if (Status != TaskStatus.Pending)
            throw new InvalidOperationException("Can only start pending tasks");

        Status = TaskStatus.InProgress;
        StartedAt = DateTime.UtcNow;
    }

    public void Complete(string? outcome = null, string? completionNotes = null, string? formData = null)
    {
        if (Status != TaskStatus.InProgress && Status != TaskStatus.Pending)
            throw new InvalidOperationException("Can only complete pending or in-progress tasks");

        Status = TaskStatus.Completed;
        CompletedAt = DateTime.UtcNow;
        Outcome = outcome;
        CompletionNotes = completionNotes;
        FormData = formData;
    }

    public void Reject(string reason)
    {
        if (Status != TaskStatus.InProgress && Status != TaskStatus.Pending)
            throw new InvalidOperationException("Can only reject pending or in-progress tasks");

        Status = TaskStatus.Rejected;
        CompletedAt = DateTime.UtcNow;
        CompletionNotes = reason;
    }

    public void Cancel(string? reason = null)
    {
        if (Status == TaskStatus.Completed || Status == TaskStatus.Rejected)
            throw new InvalidOperationException("Cannot cancel completed or rejected tasks");

        Status = TaskStatus.Cancelled;
        CompletedAt = DateTime.UtcNow;
        CompletionNotes = reason;
    }

    public void Reassign(Guid newAssignee, Guid reassignedBy, string? reason = null)
    {
        if (Status == TaskStatus.Completed || Status == TaskStatus.Rejected || Status == TaskStatus.Cancelled)
            throw new InvalidOperationException("Cannot reassign completed, rejected, or cancelled tasks");

        AssignedTo = newAssignee;
        AssignedBy = reassignedBy;
        
        // Reset to pending if it was in progress
        if (Status == TaskStatus.InProgress)
        {
            Status = TaskStatus.Pending;
            StartedAt = null;
        }
    }

    public void SetFormData(string formData)
    {
        FormData = formData;
    }

    public void UpdateDueDate(DateTime? dueDate)
    {
        DueDate = dueDate;
    }
}
