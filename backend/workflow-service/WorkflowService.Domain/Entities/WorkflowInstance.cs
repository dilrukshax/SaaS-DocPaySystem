using WorkflowService.Domain.ValueObjects;

namespace WorkflowService.Domain.Entities;

public class WorkflowInstance
{
    public Guid Id { get; private set; }
    public Guid WorkflowDefinitionId { get; private set; }
    public string EntityId { get; private set; }
    public string EntityType { get; private set; }
    public WorkflowStatus Status { get; private set; }
    public string? CurrentStepId { get; private set; }
    public Guid InitiatedBy { get; private set; }
    public DateTime InitiatedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public string? Context { get; private set; }
    public Guid TenantId { get; private set; }
    public int Priority { get; private set; }
    public DateTime? DueDate { get; private set; }

    // Navigation property
    public WorkflowDefinition WorkflowDefinition { get; private set; } = null!;

    private readonly List<Task> _tasks = new();
    public IReadOnlyList<Task> Tasks => _tasks.AsReadOnly();

    private WorkflowInstance() { } // For EF Core

    public WorkflowInstance(Guid workflowDefinitionId, string entityId, string entityType, 
        Guid initiatedBy, Guid tenantId, string? context = null)
    {
        Id = Guid.NewGuid();
        WorkflowDefinitionId = workflowDefinitionId;
        EntityId = entityId ?? throw new ArgumentNullException(nameof(entityId));
        EntityType = entityType ?? throw new ArgumentNullException(nameof(entityType));
        Status = WorkflowStatus.Running;
        InitiatedBy = initiatedBy;
        InitiatedAt = DateTime.UtcNow;
        Context = context;
        TenantId = tenantId;
        Priority = 1; // Default priority
    }

    public void CreateTask(string title, string description, Guid assignedTo, 
        TaskType taskType, int priority = 1, DateTime? dueDate = null)
    {
        if (Status != WorkflowStatus.Running)
            throw new InvalidOperationException("Cannot create tasks for non-running workflow instances");

        var task = new Task(Id, title, description, assignedTo, taskType, priority, dueDate);
        _tasks.Add(task);
    }

    public void Complete(Guid completedBy, string? outcome = null)
    {
        if (Status != WorkflowStatus.Running)
            throw new InvalidOperationException("Cannot complete non-running workflow instance");

        Status = WorkflowStatus.Completed;
        CompletedAt = DateTime.UtcNow;
        
        // Mark any pending tasks as cancelled
        foreach (var task in _tasks.Where(t => t.Status == TaskStatus.Pending || t.Status == TaskStatus.InProgress))
        {
            task.Cancel("Workflow completed");
        }
    }

    public void Cancel(Guid cancelledBy, string? reason = null)
    {
        if (Status == WorkflowStatus.Completed)
            throw new InvalidOperationException("Cannot cancel completed workflow instance");

        Status = WorkflowStatus.Cancelled;
        CompletedAt = DateTime.UtcNow;
        
        // Cancel all pending tasks
        foreach (var task in _tasks.Where(t => t.Status == TaskStatus.Pending || t.Status == TaskStatus.InProgress))
        {
            task.Cancel(reason ?? "Workflow cancelled");
        }
    }

    public void Suspend(string reason)
    {
        if (Status != WorkflowStatus.Running)
            throw new InvalidOperationException("Cannot suspend non-running workflow instance");

        Status = WorkflowStatus.Suspended;
    }

    public void Resume()
    {
        if (Status != WorkflowStatus.Suspended)
            throw new InvalidOperationException("Cannot resume non-suspended workflow instance");

        Status = WorkflowStatus.Running;
    }

    public void SetCurrentStep(string stepId)
    {
        CurrentStepId = stepId;
    }

    public void SetPriority(int priority)
    {
        Priority = Math.Max(1, Math.Min(5, priority)); // Ensure priority is between 1-5
    }

    public void SetDueDate(DateTime? dueDate)
    {
        DueDate = dueDate;
    }
}
