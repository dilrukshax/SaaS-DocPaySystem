using WorkflowService.Domain.ValueObjects;

namespace WorkflowService.Domain.Entities;

public class WorkflowDefinition
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public string WorkflowType { get; private set; }
    public int Version { get; private set; }
    public bool IsActive { get; private set; }
    public string JsonDefinition { get; private set; }
    public Guid TenantId { get; private set; }
    public Guid CreatedBy { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private readonly List<ApprovalStep> _approvalSteps = new();
    public IReadOnlyList<ApprovalStep> ApprovalSteps => _approvalSteps.AsReadOnly();

    private readonly List<WorkflowInstance> _instances = new();
    public IReadOnlyList<WorkflowInstance> Instances => _instances.AsReadOnly();

    private WorkflowDefinition() { } // For EF Core

    public WorkflowDefinition(string name, string description, string workflowType, 
        string jsonDefinition, Guid tenantId, Guid createdBy)
    {
        Id = Guid.NewGuid();
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Description = description ?? string.Empty;
        WorkflowType = workflowType ?? throw new ArgumentNullException(nameof(workflowType));
        JsonDefinition = jsonDefinition ?? throw new ArgumentNullException(nameof(jsonDefinition));
        Version = 1;
        IsActive = true;
        TenantId = tenantId;
        CreatedBy = createdBy;
        CreatedAt = DateTime.UtcNow;
    }

    public void AddApprovalStep(string name, string description, int order, 
        bool isRequired, string? approverRole = null, Guid? specificApproverId = null)
    {
        var step = new ApprovalStep(Id, name, description, order, isRequired, approverRole, specificApproverId);
        _approvalSteps.Add(step);
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateDefinition(string jsonDefinition)
    {
        JsonDefinition = jsonDefinition ?? throw new ArgumentNullException(nameof(jsonDefinition));
        UpdatedAt = DateTime.UtcNow;
    }

    public void CreateNewVersion(string jsonDefinition, Guid updatedBy)
    {
        Version++;
        JsonDefinition = jsonDefinition ?? throw new ArgumentNullException(nameof(jsonDefinition));
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public WorkflowInstance CreateInstance(string entityId, string entityType, Guid initiatedBy, string? context = null)
    {
        if (!IsActive)
            throw new InvalidOperationException("Cannot create instance from inactive workflow definition");

        var instance = new WorkflowInstance(Id, entityId, entityType, initiatedBy, TenantId, context);
        return instance;
    }
}
