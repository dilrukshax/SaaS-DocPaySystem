using Microsoft.EntityFrameworkCore;
using DocumentService.Application.DTOs;
using DocumentService.Application.Interfaces;
using DocumentService.Domain.Entities;
using DocumentService.Infrastructure.Persistence;

namespace DocumentService.Infrastructure.Repositories;

public class DocumentRepository : IDocumentRepository
{
    private readonly DocumentDbContext _context;

    public DocumentRepository(DocumentDbContext context)
    {
        _context = context;
    }

    public async Task<Document?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Documents
            .Include(d => d.Versions)
            .FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted, cancellationToken);
    }

    public async Task<PagedResult<Document>> GetPagedAsync(
        Guid tenantId,
        Guid? userId = null,
        string? status = null,
        IEnumerable<string>? tags = null,
        string? searchTerm = null,
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Documents
            .Where(d => d.TenantId == tenantId && !d.IsDeleted);

        if (userId.HasValue)
            query = query.Where(d => d.CreatedBy == userId.Value);

        if (!string.IsNullOrEmpty(status))
            query = query.Where(d => d.Status.ToString() == status);

        if (tags != null && tags.Any())
        {
            foreach (var tag in tags)
            {
                query = query.Where(d => d.Tags.Contains(tag));
            }
        }

        if (!string.IsNullOrEmpty(searchTerm))
        {
            query = query.Where(d => 
                d.Name.Contains(searchTerm) ||
                d.Description.Contains(searchTerm) ||
                d.FileName.Contains(searchTerm) ||
                (d.ExtractedText != null && d.ExtractedText.Contains(searchTerm)));
        }

        var totalCount = await query.CountAsync(cancellationToken);
        
        var items = await query
            .OrderByDescending(d => d.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Document>(items, totalCount, page, pageSize);
    }

    public async Task<IEnumerable<Document>> GetByTenantAsync(
        Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await _context.Documents
            .Where(d => d.TenantId == tenantId && !d.IsDeleted)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Document document, CancellationToken cancellationToken = default)
    {
        _context.Documents.Add(document);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Document document, CancellationToken cancellationToken = default)
    {
        _context.Documents.Update(document);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, Guid deletedBy, CancellationToken cancellationToken = default)
    {
        var document = await GetByIdAsync(id, cancellationToken);
        if (document != null)
        {
            document.SoftDelete(deletedBy);
            await UpdateAsync(document, cancellationToken);
        }
    }

    public async Task<IEnumerable<DocumentVersion>> GetVersionsAsync(
        Guid documentId, CancellationToken cancellationToken = default)
    {
        return await _context.DocumentVersions
            .Where(v => EF.Property<Guid>(v, "DocumentId") == documentId)
            .OrderByDescending(v => v.VersionNumber)
            .ToListAsync(cancellationToken);
    }
}
