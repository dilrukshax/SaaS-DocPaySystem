namespace WorkflowService.Domain.Entities;

public class ApprovalStep
{
    public Guid Id { get; private set; }
    public Guid WorkflowDefinitionId { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public int Order { get; private set; }
    public bool IsRequired { get; private set; }
    public string? ApproverRole { get; private set; }
    public Guid? SpecificApproverId { get; private set; }
    public int TimeoutDays { get; private set; }
    public string? EscalationRole { get; private set; }
    public Guid? EscalationUserId { get; private set; }

    // Navigation property
    public WorkflowDefinition WorkflowDefinition { get; private set; } = null!;

    private ApprovalStep() { } // For EF Core

    public ApprovalStep(Guid workflowDefinitionId, string name, string description, int order, 
        bool isRequired, string? approverRole = null, Guid? specificApproverId = null)
    {
        Id = Guid.NewGuid();
        WorkflowDefinitionId = workflowDefinitionId;
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Description = description ?? string.Empty;
        Order = order;
        IsRequired = isRequired;
        ApproverRole = approverRole;
        SpecificApproverId = specificApproverId;
        TimeoutDays = 7; // Default timeout
    }

    public void SetTimeout(int timeoutDays)
    {
        TimeoutDays = Math.Max(1, timeoutDays);
    }

    public void SetEscalation(string? escalationRole = null, Guid? escalationUserId = null)
    {
        EscalationRole = escalationRole;
        EscalationUserId = escalationUserId;
    }

    public void UpdateApprover(string? approverRole = null, Guid? specificApproverId = null)
    {
        ApproverRole = approverRole;
        SpecificApproverId = specificApproverId;
    }
}
