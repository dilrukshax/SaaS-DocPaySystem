using MediatR;
using DocumentService.Application.DTOs;
using DocumentService.Application.Interfaces;
using DocumentService.Application.Queries;

namespace DocumentService.Application.Handlers;

public class GetDocumentHandler : IRequestHandler<GetDocumentQuery, DocumentDto?>
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IBlobStorageService _blobStorageService;

    public GetDocumentHandler(
        IDocumentRepository documentRepository,
        IBlobStorageService blobStorageService)
    {
        _documentRepository = documentRepository;
        _blobStorageService = blobStorageService;
    }

    public async Task<DocumentDto?> Handle(GetDocumentQuery request, CancellationToken cancellationToken)
    {
        var document = await _documentRepository.GetByIdAsync(request.Id, cancellationToken);
        if (document == null) return null;

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

public class GetDocumentVersionsHandler : IRequestHandler<GetDocumentVersionsQuery, IEnumerable<DocumentVersionDto>>
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IBlobStorageService _blobStorageService;

    public GetDocumentVersionsHandler(
        IDocumentRepository documentRepository,
        IBlobStorageService blobStorageService)
    {
        _documentRepository = documentRepository;
        _blobStorageService = blobStorageService;
    }

    public async Task<IEnumerable<DocumentVersionDto>> Handle(GetDocumentVersionsQuery request, CancellationToken cancellationToken)
    {
        var versions = await _documentRepository.GetVersionsAsync(request.DocumentId, cancellationToken);
        var versionDtos = new List<DocumentVersionDto>();

        foreach (var version in versions)
        {
            var downloadUrl = await _blobStorageService.GenerateDownloadUrlAsync(
                version.StoragePath, TimeSpan.FromHours(1), cancellationToken);

            versionDtos.Add(new DocumentVersionDto(
                version.Id,
                version.VersionNumber,
                version.FileName,
                version.MimeType,
                version.FileSize,
                version.Comment,
                version.CreatedAt,
                downloadUrl));
        }

        return versionDtos;
    }
}

public class GetDocumentUrlHandler : IRequestHandler<GetDocumentUrlQuery, DocumentUrlDto?>
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IBlobStorageService _blobStorageService;

    public GetDocumentUrlHandler(
        IDocumentRepository documentRepository,
        IBlobStorageService blobStorageService)
    {
        _documentRepository = documentRepository;
        _blobStorageService = blobStorageService;
    }

    public async Task<DocumentUrlDto?> Handle(GetDocumentUrlQuery request, CancellationToken cancellationToken)
    {
        var document = await _documentRepository.GetByIdAsync(request.Id, cancellationToken);
        if (document == null) return null;

        var downloadUrl = await _blobStorageService.GenerateDownloadUrlAsync(
            document.StoragePath, TimeSpan.FromHours(24), cancellationToken);

        return new DocumentUrlDto(downloadUrl, DateTime.UtcNow.AddHours(24));
    }
}
