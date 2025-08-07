namespace DocumentService.Application.DTOs;

public record UploadDocumentRequest(
    string Name,
    string Description,
    IFormFile File,
    Guid TenantId,
    Guid UserId,
    string? Tags = null
);

public record DocumentDto(
    Guid Id,
    string Name,
    string Description,
    string FileName,
    string MimeType,
    long FileSize,
    string Status,
    int CurrentVersion,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    string? Tags,
    string Url
);

public record DocumentVersionDto(
    Guid Id,
    int VersionNumber,
    long FileSize,
    string? ChangeLog,
    DateTime CreatedAt,
    string Url
);

public record UpdateDocumentRequest(
    string Name,
    string Description,
    string? Tags = null
);

public record OCRResult(
    Guid DocumentId,
    string ExtractedText,
    float Confidence,
    Dictionary<string, object>? Metadata,
    DateTime ProcessedAt
);
