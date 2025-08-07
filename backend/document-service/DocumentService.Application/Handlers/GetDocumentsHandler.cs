using MediatR;
using DocumentService.Application.DTOs;
using DocumentService.Application.Interfaces;
using DocumentService.Application.Queries;

namespace DocumentService.Application.Handlers;

public class GetDocumentsHandler : IRequestHandler<GetDocumentsQuery, PagedResult<DocumentDto>>
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IBlobStorageService _blobStorageService;

    public GetDocumentsHandler(
        IDocumentRepository documentRepository,
        IBlobStorageService blobStorageService)
    {
        _documentRepository = documentRepository;
        _blobStorageService = blobStorageService;
    }

    public async Task<PagedResult<DocumentDto>> Handle(GetDocumentsQuery request, CancellationToken cancellationToken)
    {
        var documents = await _documentRepository.GetPagedAsync(
            request.TenantId,
            request.UserId,
            request.Status,
            request.Tags,
            request.SearchTerm,
            request.Page,
            request.PageSize,
            cancellationToken);

        var documentDtos = new List<DocumentDto>();

        foreach (var document in documents.Items)
        {
            var downloadUrl = await _blobStorageService.GenerateDownloadUrlAsync(
                document.StoragePath, TimeSpan.FromHours(1), cancellationToken);

            documentDtos.Add(new DocumentDto(
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
                downloadUrl));
        }

        return new PagedResult<DocumentDto>(
            documentDtos,
            documents.TotalCount,
            documents.Page,
            documents.PageSize);
    }
}
