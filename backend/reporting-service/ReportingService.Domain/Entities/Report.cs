using ReportingService.Domain.ValueObjects;

namespace ReportingService.Domain.Entities;

public class Report
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public ReportType Type { get; private set; }
    public string QueryDefinition { get; private set; }
    public string? Parameters { get; private set; }
    public ReportFormat DefaultFormat { get; private set; }
    public Guid TenantId { get; private set; }
    public Guid CreatedBy { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public bool IsActive { get; private set; }
    public string? Tags { get; private set; }

    private readonly List<ReportRequest> _requests = new();
    public IReadOnlyList<ReportRequest> Requests => _requests.AsReadOnly();

    private readonly List<ReportSchedule> _schedules = new();
    public IReadOnlyList<ReportSchedule> Schedules => _schedules.AsReadOnly();

    private Report() { } // For EF Core

    public Report(string name, string description, ReportType type, string queryDefinition, 
        ReportFormat defaultFormat, Guid tenantId, Guid createdBy, string? parameters = null, string? tags = null)
    {
        Id = Guid.NewGuid();
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Description = description ?? string.Empty;
        Type = type;
        QueryDefinition = queryDefinition ?? throw new ArgumentNullException(nameof(queryDefinition));
        Parameters = parameters;
        DefaultFormat = defaultFormat;
        TenantId = tenantId;
        CreatedBy = createdBy;
        CreatedAt = DateTime.UtcNow;
        IsActive = true;
        Tags = tags;
    }

    public void UpdateDefinition(string name, string description, string queryDefinition, 
        string? parameters = null, string? tags = null)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Description = description ?? string.Empty;
        QueryDefinition = queryDefinition ?? throw new ArgumentNullException(nameof(queryDefinition));
        Parameters = parameters;
        Tags = tags;
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

    public ReportRequest CreateRequest(Guid requestedBy, ReportFormat format, 
        string? runtimeParameters = null)
    {
        if (!IsActive)
            throw new InvalidOperationException("Cannot create request for inactive report");

        var request = new ReportRequest(Id, requestedBy, format, runtimeParameters);
        return request;
    }

    public ReportSchedule CreateSchedule(string cronExpression, ReportFormat format, 
        string recipientEmails, Guid createdBy, string? runtimeParameters = null)
    {
        var schedule = new ReportSchedule(Id, cronExpression, format, recipientEmails, createdBy, runtimeParameters);
        _schedules.Add(schedule);
        UpdatedAt = DateTime.UtcNow;
        return schedule;
    }
}
