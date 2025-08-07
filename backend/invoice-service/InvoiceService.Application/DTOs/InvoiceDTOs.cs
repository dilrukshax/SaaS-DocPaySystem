namespace InvoiceService.Application.DTOs;

// Request DTOs
public record CreateInvoiceRequest(
    Guid DocumentId,
    Guid CustomerId,
    decimal Amount,
    string Currency,
    DateTime DueDate,
    List<InvoiceLineItemDto> LineItems,
    Guid TenantId,
    Guid CreatedBy);

public record UpdateInvoiceRequest(
    string? Description,
    DateTime DueDate,
    List<InvoiceLineItemDto> LineItems,
    Guid UpdatedBy);

public record ProcessPaymentRequest(
    decimal Amount,
    string PaymentMethod,
    string? Reference,
    Guid ProcessedBy);

// Response DTOs
public record InvoiceDto(
    Guid Id,
    string InvoiceNumber,
    Guid DocumentId,
    Guid CustomerId,
    decimal Amount,
    string Currency,
    string Status,
    DateTime IssueDate,
    DateTime DueDate,
    decimal PaidAmount,
    decimal OutstandingAmount,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    List<InvoiceLineItemDto> LineItems);

public record InvoiceLineItemDto(
    Guid Id,
    string Description,
    decimal Quantity,
    decimal UnitPrice,
    decimal Amount,
    string? TaxCode,
    decimal TaxRate,
    decimal TaxAmount);

public record InvoicePaymentDto(
    Guid Id,
    decimal Amount,
    string PaymentMethod,
    string Status,
    string? Reference,
    DateTime ProcessedAt,
    Guid ProcessedBy);

// Paging result
public record PagedResult<T>(
    IEnumerable<T> Items,
    int TotalCount,
    int Page,
    int PageSize)
{
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;
}
