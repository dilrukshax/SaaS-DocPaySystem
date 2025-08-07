using InvoiceService.Application.DTOs;
using InvoiceService.Domain.Entities;

namespace InvoiceService.Application.Interfaces;

// Repository Interfaces
public interface IInvoiceRepository
{
    Task<Invoice?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PagedResult<Invoice>> GetPagedAsync(
        Guid tenantId,
        Guid? customerId = null,
        string? status = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? searchTerm = null,
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default);
    Task<IEnumerable<Invoice>> GetByTenantAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Invoice>> GetByCustomerAsync(Guid customerId, Guid tenantId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Invoice>> GetOverdueAsync(Guid tenantId, int daysOverdue = 0, CancellationToken cancellationToken = default);
    Task<Invoice?> GetByInvoiceNumberAsync(string invoiceNumber, Guid tenantId, CancellationToken cancellationToken = default);
    Task AddAsync(Invoice invoice, CancellationToken cancellationToken = default);
    Task UpdateAsync(Invoice invoice, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, Guid deletedBy, CancellationToken cancellationToken = default);
    Task<IEnumerable<InvoicePayment>> GetPaymentsAsync(Guid invoiceId, CancellationToken cancellationToken = default);
    Task<InvoiceStatisticsDto> GetStatisticsAsync(Guid tenantId, DateTime? fromDate, DateTime? toDate, CancellationToken cancellationToken = default);
}

// Service Interfaces
public interface IInvoiceNumberService
{
    Task<string> GenerateInvoiceNumberAsync(Guid tenantId, CancellationToken cancellationToken = default);
}

public interface IEmailService
{
    Task<bool> SendInvoiceAsync(
        string recipientEmail,
        string invoiceNumber,
        byte[] invoicePdf,
        CancellationToken cancellationToken = default);
    Task<bool> SendOverdueNoticeAsync(
        string recipientEmail,
        string invoiceNumber,
        decimal amount,
        int daysOverdue,
        CancellationToken cancellationToken = default);
}

public interface IPdfGenerationService
{
    Task<byte[]> GenerateInvoicePdfAsync(
        Invoice invoice,
        CancellationToken cancellationToken = default);
    Task<byte[]> GenerateInvoicePdfFromTemplateAsync(
        Invoice invoice,
        Guid templateId,
        CancellationToken cancellationToken = default);
}

public interface IPaymentGatewayService
{
    Task<PaymentResult> ProcessPaymentAsync(
        string paymentMethod,
        decimal amount,
        string currency,
        string? reference,
        CancellationToken cancellationToken = default);
    Task<PaymentResult> RefundPaymentAsync(
        string paymentReference,
        decimal amount,
        CancellationToken cancellationToken = default);
}

public interface IEventPublisher
{
    Task PublishAsync<T>(T @event, CancellationToken cancellationToken = default) where T : class;
    Task PublishBatchAsync<T>(IEnumerable<T> events, CancellationToken cancellationToken = default) where T : class;
}

// Supporting Types
public record PaymentResult(
    bool IsSuccess,
    string? PaymentReference,
    string? ErrorMessage,
    DateTime ProcessedAt);
