using MediatR;
using InvoiceService.Application.DTOs;

namespace InvoiceService.Application.Queries;

// Get Invoice by ID Query
public record GetInvoiceQuery(Guid Id) : IRequest<InvoiceDto?>;

// Get Invoices with Pagination Query
public record GetInvoicesQuery(
    Guid TenantId,
    Guid? CustomerId = null,
    string? Status = null,
    DateTime? FromDate = null,
    DateTime? ToDate = null,
    string? SearchTerm = null,
    int Page = 1,
    int PageSize = 10) : IRequest<PagedResult<InvoiceDto>>;

// Get Customer Invoices Query
public record GetCustomerInvoicesQuery(
    Guid CustomerId,
    Guid TenantId,
    string? Status = null,
    int Page = 1,
    int PageSize = 10) : IRequest<PagedResult<InvoiceDto>>;

// Get Invoice Payments Query
public record GetInvoicePaymentsQuery(
    Guid InvoiceId) : IRequest<IEnumerable<InvoicePaymentDto>>;

// Get Overdue Invoices Query
public record GetOverdueInvoicesQuery(
    Guid TenantId,
    int DaysOverdue = 0,
    int Page = 1,
    int PageSize = 10) : IRequest<PagedResult<InvoiceDto>>;

// Get Invoice Statistics Query
public record GetInvoiceStatisticsQuery(
    Guid TenantId,
    DateTime? FromDate = null,
    DateTime? ToDate = null) : IRequest<InvoiceStatisticsDto>;

// Search Invoices Query
public record SearchInvoicesQuery(
    Guid TenantId,
    string SearchTerm,
    int Page = 1,
    int PageSize = 10) : IRequest<PagedResult<InvoiceDto>>;

// Statistics DTO
public record InvoiceStatisticsDto(
    int TotalInvoices,
    decimal TotalAmount,
    decimal PaidAmount,
    decimal OutstandingAmount,
    decimal OverdueAmount,
    int OverdueCount,
    Dictionary<string, int> StatusCounts,
    Dictionary<string, decimal> MonthlyTotals);
