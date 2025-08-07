using MediatR;
using DocumentService.Application.Commands;
using DocumentService.Application.DTOs;
using DocumentService.Application.Events;
using DocumentService.Application.Interfaces;
using DocumentService.Domain.Entities;

namespace DocumentService.Application.Handlers;

public class UpdateDocumentHandler : IRequestHandler<UpdateDocumentCommand, DocumentDto?>
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IBlobStorageService _blobStorageService;

    public UpdateDocumentHandler(
        IDocumentRepository documentRepository,
        IBlobStorageService blobStorageService)
    {
        _documentRepository = documentRepository;
        _blobStorageService = blobStorageService;
    }

    public async Task<DocumentDto?> Handle(UpdateDocumentCommand request, CancellationToken cancellationToken)
    {
        var document = await _documentRepository.GetByIdAsync(request.Id, cancellationToken);
        if (document == null) return null;

        document.UpdateMetadata(request.Name, request.Description, request.Tags, request.UpdatedBy);
        await _documentRepository.UpdateAsync(document, cancellationToken);

        var downloadUrl = await _blobStorageService.GenerateDownloadUrlAsync(
            document.StoragePath, TimeSpan.FromHours(1), cancellationToken);

        return new DocumentDto(
            document.Id,
            document.Name,
            document.Description,
            document.FileName,
            document.MimeType,
            document.FileSize,
            document.Status.ToString(),
            document.CurrentVersion,
            document.CreatedAt,
            document.UpdatedAt,
            document.Tags,
            downloadUrl);
    }
}

public class DeleteDocumentHandler : IRequestHandler<DeleteDocumentCommand>
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IEventPublisher _eventPublisher;

    public DeleteDocumentHandler(
        IDocumentRepository documentRepository,
        IEventPublisher eventPublisher)
    {
        _documentRepository = documentRepository;
        _eventPublisher = eventPublisher;
    }

    public async Task Handle(DeleteDocumentCommand request, CancellationToken cancellationToken)
    {
        var document = await _documentRepository.GetByIdAsync(request.Id, cancellationToken);
        if (document != null)
        {
            await _documentRepository.DeleteAsync(request.Id, request.DeletedBy, cancellationToken);

            await _eventPublisher.PublishAsync(new DocumentDeletedEvent(
                document.Id,
                document.Name,
                document.TenantId,
                request.DeletedBy,
                DateTime.UtcNow), cancellationToken);
        }
    }
}

public class AddDocumentVersionHandler : IRequestHandler<AddDocumentVersionCommand, DocumentVersionDto?>
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IBlobStorageService _blobStorageService;
    private readonly IEventPublisher _eventPublisher;

    public AddDocumentVersionHandler(
        IDocumentRepository documentRepository,
        IBlobStorageService blobStorageService,
        IEventPublisher eventPublisher)
    {
        _documentRepository = documentRepository;
        _blobStorageService = blobStorageService;
        _eventPublisher = eventPublisher;
    }

    public async Task<DocumentVersionDto?> Handle(AddDocumentVersionCommand request, CancellationToken cancellationToken)
    {
        var document = await _documentRepository.GetByIdAsync(request.DocumentId, cancellationToken);
        if (document == null) return null;

        // Upload new version to blob storage
        var storagePath = await _blobStorageService.UploadAsync(
            request.FileStream, request.FileName, request.MimeType, cancellationToken);

        // Add version to document
        var version = document.AddVersion(
            request.FileName, request.MimeType, request.FileSize, storagePath, request.Comment, request.CreatedBy);

        await _documentRepository.UpdateAsync(document, cancellationToken);

        // Publish event
        await _eventPublisher.PublishAsync(new DocumentVersionAddedEvent(
            document.Id,
            version.Id,
            version.VersionNumber,
            document.TenantId,
            request.CreatedBy,
            DateTime.UtcNow), cancellationToken);

        var downloadUrl = await _blobStorageService.GenerateDownloadUrlAsync(
            storagePath, TimeSpan.FromHours(1), cancellationToken);

        return new DocumentVersionDto(
            version.Id,
            version.VersionNumber,
            version.FileName,
            version.MimeType,
            version.FileSize,
            version.Comment,
            version.CreatedAt,
            downloadUrl);
    }
}
