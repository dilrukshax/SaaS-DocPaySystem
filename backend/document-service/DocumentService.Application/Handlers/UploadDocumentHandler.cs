using MediatR;
using DocumentService.Application.Commands;
using DocumentService.Application.DTOs;
using DocumentService.Application.Events;
using DocumentService.Application.Interfaces;
using DocumentService.Domain.Entities;

namespace DocumentService.Application.Handlers;

public class UploadDocumentHandler : IRequestHandler<UploadDocumentCommand, DocumentDto>
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IBlobStorageService _blobStorageService;
    private readonly IEventPublisher _eventPublisher;

    public UploadDocumentHandler(
        IDocumentRepository documentRepository,
        IBlobStorageService blobStorageService,
        IEventPublisher eventPublisher)
    {
        _documentRepository = documentRepository;
        _blobStorageService = blobStorageService;
        _eventPublisher = eventPublisher;
    }

    public async Task<DocumentDto> Handle(UploadDocumentCommand request, CancellationToken cancellationToken)
    {
        // Upload file to blob storage
        var storagePath = await _blobStorageService.UploadAsync(
            request.FileStream, request.FileName, request.MimeType, cancellationToken);

        // Create document entity
        var document = new Document(
            request.Name,
            request.Description,
            request.FileName,
            request.MimeType,
            request.FileSize,
            storagePath,
            request.TenantId,
            request.UserId,
            request.Tags);

        // Save to repository
        await _documentRepository.AddAsync(document, cancellationToken);

        // Publish event
        await _eventPublisher.PublishAsync(new DocumentUploadedEvent(
            document.Id,
            document.Name,
            document.MimeType,
            document.FileSize,
            document.TenantId,
            document.CreatedBy,
            document.CreatedAt), cancellationToken);

        // Generate download URL
        var downloadUrl = await _blobStorageService.GenerateDownloadUrlAsync(
            storagePath, TimeSpan.FromHours(1), cancellationToken);

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
