using MediatR;

namespace DocumentService.Application.Events;

public record DocumentUploadedEvent(
    Guid DocumentId,
    string DocumentName,
    string MimeType,
    long FileSize,
    Guid TenantId,
    Guid UploadedBy,
    DateTime UploadedAt
) : INotification;

public record DocumentDeletedEvent(
    Guid DocumentId,
    string DocumentName,
    Guid TenantId,
    Guid DeletedBy,
    DateTime DeletedAt
) : INotification;

public record DocumentVersionAddedEvent(
    Guid DocumentId,
    int VersionNumber,
    string ChangeLog,
    Guid TenantId,
    Guid AddedBy,
    DateTime AddedAt
) : INotification;

public record OCRProcessedEvent(
    Guid DocumentId,
    string ExtractedText,
    float Confidence,
    Guid TenantId,
    DateTime ProcessedAt
) : INotification;
