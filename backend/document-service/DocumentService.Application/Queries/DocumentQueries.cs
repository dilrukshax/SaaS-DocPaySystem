using MediatR;
using DocumentService.Application.DTOs;

namespace DocumentService.Application.Queries;

public record GetDocumentQuery(
    Guid Id,
    Guid TenantId
) : IRequest<DocumentDto?>;

public record GetDocumentsQuery(
    Guid TenantId,
    int Page = 1,
    int PageSize = 10,
    string? Search = null,
    string[]? Tags = null
) : IRequest<PagedResult<DocumentDto>>;

public record GetDocumentVersionsQuery(
    Guid DocumentId,
    Guid TenantId
) : IRequest<IEnumerable<DocumentVersionDto>>;

public record GetDocumentUrlQuery(
    Guid DocumentId,
    int? Version,
    Guid TenantId
) : IRequest<string>;

public record PagedResult<T>(
    IEnumerable<T> Items,
    int TotalCount,
    int Page,
    int PageSize
)
{
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;
}
