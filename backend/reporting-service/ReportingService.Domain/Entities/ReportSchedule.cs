using ReportingService.Domain.ValueObjects;

namespace ReportingService.Domain.Entities;

public class ReportSchedule
{
    public Guid Id { get; private set; }
    public Guid ReportId { get; private set; }
    public string CronExpression { get; private set; }
    public ReportFormat Format { get; private set; }
    public string RecipientEmails { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime? LastExecutedAt { get; private set; }
    public DateTime? NextExecutionAt { get; private set; }
    public string? Parameters { get; private set; }
    public Guid CreatedBy { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    // Navigation property
    public Report Report { get; private set; } = null!;

    private readonly List<ScheduleExecution> _executions = new();
    public IReadOnlyList<ScheduleExecution> Executions => _executions.AsReadOnly();

    private ReportSchedule() { } // For EF Core

    public ReportSchedule(Guid reportId, string cronExpression, ReportFormat format, 
        string recipientEmails, Guid createdBy, string? parameters = null)
    {
        Id = Guid.NewGuid();
        ReportId = reportId;
        CronExpression = cronExpression ?? throw new ArgumentNullException(nameof(cronExpression));
        Format = format;
        RecipientEmails = recipientEmails ?? throw new ArgumentNullException(nameof(recipientEmails));
        Parameters = parameters;
        CreatedBy = createdBy;
        CreatedAt = DateTime.UtcNow;
        IsActive = true;
        
        CalculateNextExecution();
    }

    public void UpdateSchedule(string cronExpression, string recipientEmails, string? parameters = null)
    {
        CronExpression = cronExpression ?? throw new ArgumentNullException(nameof(cronExpression));
        RecipientEmails = recipientEmails ?? throw new ArgumentNullException(nameof(recipientEmails));
        Parameters = parameters;
        UpdatedAt = DateTime.UtcNow;
        
        CalculateNextExecution();
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
        CalculateNextExecution();
    }

    public void Deactivate()
    {
        IsActive = false;
        NextExecutionAt = null;
        UpdatedAt = DateTime.UtcNow;
    }

    public void RecordExecution(bool success, string? errorMessage = null, string? outputPath = null)
    {
        LastExecutedAt = DateTime.UtcNow;
        
        var execution = new ScheduleExecution(Id, success, errorMessage, outputPath);
        _executions.Add(execution);
        
        if (IsActive)
        {
            CalculateNextExecution();
        }
    }

    private void CalculateNextExecution()
    {
        // Simple implementation - in real scenario, use a cron parser library
        // For demo purposes, just add 1 day if daily schedule
        if (CronExpression.Contains("0 8 * * *")) // Daily at 8 AM
        {
            NextExecutionAt = DateTime.UtcNow.Date.AddDays(1).AddHours(8);
        }
        else if (CronExpression.Contains("0 8 * * 1")) // Weekly on Monday at 8 AM
        {
            var daysUntilMonday = ((int)DayOfWeek.Monday - (int)DateTime.UtcNow.DayOfWeek + 7) % 7;
            if (daysUntilMonday == 0) daysUntilMonday = 7; // If today is Monday, schedule for next Monday
            NextExecutionAt = DateTime.UtcNow.Date.AddDays(daysUntilMonday).AddHours(8);
        }
        else
        {
            NextExecutionAt = DateTime.UtcNow.AddHours(1); // Default to 1 hour from now
        }
    }
}

public class ScheduleExecution
{
    public Guid Id { get; private set; }
    public Guid ScheduleId { get; private set; }
    public DateTime ExecutedAt { get; private set; }
    public bool Success { get; private set; }
    public string? ErrorMessage { get; private set; }
    public string? OutputPath { get; private set; }
    public TimeSpan? Duration { get; private set; }

    // Navigation property
    public ReportSchedule Schedule { get; private set; } = null!;

    private ScheduleExecution() { } // For EF Core

    public ScheduleExecution(Guid scheduleId, bool success, string? errorMessage = null, string? outputPath = null)
    {
        Id = Guid.NewGuid();
        ScheduleId = scheduleId;
        ExecutedAt = DateTime.UtcNow;
        Success = success;
        ErrorMessage = errorMessage;
        OutputPath = outputPath;
    }

    public void SetDuration(TimeSpan duration)
    {
        Duration = duration;
    }
}
