using MediatR;
using DocumentService.Application.DTOs;

namespace DocumentService.Application.Commands;

public record UploadDocumentCommand(
    string Name,
    string Description,
    Stream FileStream,
    string FileName,
    string MimeType,
    long FileSize,
    Guid TenantId,
    Guid UserId,
    string? Tags = null
) : IRequest<DocumentDto>;

public record UpdateDocumentCommand(
    Guid Id,
    string Name,
    string Description,
    Guid UserId,
    string? Tags = null
) : IRequest<DocumentDto>;

public record DeleteDocumentCommand(
    Guid Id,
    Guid UserId
) : IRequest<bool>;

public record AddDocumentVersionCommand(
    Guid DocumentId,
    Stream FileStream,
    string FileName,
    long FileSize,
    string ChangeLog,
    Guid UserId
) : IRequest<DocumentVersionDto>;

public record ProcessOCRCommand(
    Guid DocumentId,
    Guid UserId
) : IRequest<OCRResult>;
