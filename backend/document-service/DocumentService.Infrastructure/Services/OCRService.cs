using Azure;
using Azure.AI.FormRecognizer.DocumentAnalysis;
using DocumentService.Application.DTOs;
using DocumentService.Application.Interfaces;

namespace DocumentService.Infrastructure.Services;

public class OCRService : IOCRService
{
    private readonly DocumentAnalysisClient _documentAnalysisClient;

    public OCRService(DocumentAnalysisClient documentAnalysisClient)
    {
        _documentAnalysisClient = documentAnalysisClient;
    }

    public async Task<OCRResult> ProcessAsync(
        string storagePath, 
        string language = "en", 
        CancellationToken cancellationToken = default)
    {
        try
        {
            // For Azure Form Recognizer, we'll use the prebuilt-read model for text extraction
            var operation = await _documentAnalysisClient.AnalyzeDocumentFromUriAsync(
                WaitUntil.Completed,
                "prebuilt-read",
                new Uri(storagePath),
                cancellationToken: cancellationToken);

            var result = operation.Value;
            
            var extractedText = string.Join("\n", result.Pages.SelectMany(page => 
                page.Lines.Select(line => line.Content)));

            // Calculate confidence as average of all text elements
            var confidenceValues = result.Pages
                .SelectMany(page => page.Lines)
                .SelectMany(line => line.Words)
                .Where(word => word.Confidence.HasValue)
                .Select(word => word.Confidence.Value);

            var averageConfidence = confidenceValues.Any() 
                ? confidenceValues.Average() 
                : 0.0;

            return new OCRResult(
                extractedText,
                averageConfidence,
                language,
                DateTime.UtcNow);
        }
        catch (RequestFailedException ex)
        {
            throw new InvalidOperationException($"OCR processing failed: {ex.Message}", ex);
        }
    }

    public async Task<OCRResult> ProcessStreamAsync(
        Stream documentStream, 
        string language = "en", 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var operation = await _documentAnalysisClient.AnalyzeDocumentAsync(
                WaitUntil.Completed,
                "prebuilt-read",
                documentStream,
                cancellationToken: cancellationToken);

            var result = operation.Value;
            
            var extractedText = string.Join("\n", result.Pages.SelectMany(page => 
                page.Lines.Select(line => line.Content)));

            var confidenceValues = result.Pages
                .SelectMany(page => page.Lines)
                .SelectMany(line => line.Words)
                .Where(word => word.Confidence.HasValue)
                .Select(word => word.Confidence.Value);

            var averageConfidence = confidenceValues.Any() 
                ? confidenceValues.Average() 
                : 0.0;

            return new OCRResult(
                extractedText,
                averageConfidence,
                language,
                DateTime.UtcNow);
        }
        catch (RequestFailedException ex)
        {
            throw new InvalidOperationException($"OCR processing failed: {ex.Message}", ex);
        }
    }
}
