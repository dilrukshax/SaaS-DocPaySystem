using MediatR;
using DocumentService.Application.Commands;
using DocumentService.Application.DTOs;
using DocumentService.Application.Events;
using DocumentService.Application.Interfaces;

namespace DocumentService.Application.Handlers;

public class ProcessOCRHandler : IRequestHandler<ProcessOCRCommand, OCRResult>
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IOCRService _ocrService;
    private readonly IEventPublisher _eventPublisher;

    public ProcessOCRHandler(
        IDocumentRepository documentRepository,
        IOCRService ocrService,
        IEventPublisher eventPublisher)
    {
        _documentRepository = documentRepository;
        _ocrService = ocrService;
        _eventPublisher = eventPublisher;
    }

    public async Task<OCRResult> Handle(ProcessOCRCommand request, CancellationToken cancellationToken)
    {
        var document = await _documentRepository.GetByIdAsync(request.DocumentId, cancellationToken);
        if (document == null)
            throw new InvalidOperationException($"Document {request.DocumentId} not found");

        // Process OCR
        var ocrResult = await _ocrService.ProcessAsync(
            document.StoragePath, request.Language, cancellationToken);

        // Update document with OCR data
        document.UpdateOCRData(ocrResult.ExtractedText, ocrResult.Confidence);
        await _documentRepository.UpdateAsync(document, cancellationToken);

        // Publish event
        await _eventPublisher.PublishAsync(new OCRProcessedEvent(
            document.Id,
            ocrResult.ExtractedText,
            ocrResult.Confidence,
            document.TenantId), cancellationToken);

        return new OCRResult(
            ocrResult.ExtractedText,
            ocrResult.Confidence,
            ocrResult.Language,
            ocrResult.ProcessedAt);
    }
}
