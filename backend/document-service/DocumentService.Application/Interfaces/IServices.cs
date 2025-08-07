using DocumentService.Domain.Entities;

namespace DocumentService.Application.Interfaces;

public interface IDocumentRepository
{
    Task<Document?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Document>> GetByTenantAsync(Guid tenantId, int skip, int take, 
        string? search = null, string[]? tags = null, CancellationToken cancellationToken = default);
    Task<int> GetCountByTenantAsync(Guid tenantId, string? search = null, string[]? tags = null, 
        CancellationToken cancellationToken = default);
    Task<Document> AddAsync(Document document, CancellationToken cancellationToken = default);
    Task<Document> UpdateAsync(Document document, CancellationToken cancellationToken = default);
    Task DeleteAsync(Document document, CancellationToken cancellationToken = default);
}

public interface IBlobStorageService
{
    Task<string> UploadAsync(Stream fileStream, string fileName, string mimeType, 
        CancellationToken cancellationToken = default);
    Task<Stream> DownloadAsync(string path, CancellationToken cancellationToken = default);
    Task DeleteAsync(string path, CancellationToken cancellationToken = default);
    Task<string> GenerateDownloadUrlAsync(string path, TimeSpan expiry, 
        CancellationToken cancellationToken = default);
}

public interface IOCRService
{
    Task<(string ExtractedText, float Confidence, Dictionary<string, object>? Metadata)> 
        ProcessDocumentAsync(Stream fileStream, string mimeType, CancellationToken cancellationToken = default);
}

public interface IEventPublisher
{
    Task PublishAsync<T>(T @event, CancellationToken cancellationToken = default) where T : class;
}
