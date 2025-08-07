using ReportingService.Domain.ValueObjects;

namespace ReportingService.Domain.Entities;

public class ReportRequest
{
    public Guid Id { get; private set; }
    public Guid ReportId { get; private set; }
    public Guid RequestedBy { get; private set; }
    public ReportFormat Format { get; private set; }
    public RequestStatus Status { get; private set; }
    public DateTime RequestedAt { get; private set; }
    public DateTime? StartedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public string? Parameters { get; private set; }
    public string? OutputPath { get; private set; }
    public string? ErrorMessage { get; private set; }
    public long? FileSizeBytes { get; private set; }
    public int? RecordCount { get; private set; }

    // Navigation property
    public Report Report { get; private set; } = null!;

    private ReportRequest() { } // For EF Core

    public ReportRequest(Guid reportId, Guid requestedBy, ReportFormat format, string? parameters = null)
    {
        Id = Guid.NewGuid();
        ReportId = reportId;
        RequestedBy = requestedBy;
        Format = format;
        Parameters = parameters;
        Status = RequestStatus.Pending;
        RequestedAt = DateTime.UtcNow;
    }

    public void Start()
    {
        if (Status != RequestStatus.Pending)
            throw new InvalidOperationException("Can only start pending requests");

        Status = RequestStatus.Processing;
        StartedAt = DateTime.UtcNow;
    }

    public void Complete(string outputPath, long fileSizeBytes, int recordCount)
    {
        if (Status != RequestStatus.Processing)
            throw new InvalidOperationException("Can only complete processing requests");

        Status = RequestStatus.Completed;
        CompletedAt = DateTime.UtcNow;
        OutputPath = outputPath;
        FileSizeBytes = fileSizeBytes;
        RecordCount = recordCount;
    }

    public void Fail(string errorMessage)
    {
        Status = RequestStatus.Failed;
        CompletedAt = DateTime.UtcNow;
        ErrorMessage = errorMessage;
    }

    public void Cancel()
    {
        if (Status == RequestStatus.Completed)
            throw new InvalidOperationException("Cannot cancel completed requests");

        Status = RequestStatus.Cancelled;
        CompletedAt = DateTime.UtcNow;
    }
}
