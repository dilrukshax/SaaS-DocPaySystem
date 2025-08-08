using Microsoft.EntityFrameworkCore;
using PaymentService.Application.DTOs;
using PaymentService.Application.Interfaces;
using PaymentService.Domain.Entities;
using PaymentService.Infrastructure.Persistence;

namespace PaymentService.Infrastructure.Repositories;

public class PaymentRepository : IPaymentRepository
{
    private readonly PaymentDbContext _context;

    public PaymentRepository(PaymentDbContext context)
    {
        _context = context;
    }

    public async Task<Payment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Payments
            .Include(p => p.Refunds)
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted, cancellationToken);
    }

    public async Task<PagedResult<Payment>> GetPagedAsync(
        Guid tenantId,
        Guid? invoiceId = null,
        string? status = null,
        string? paymentMethod = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Payments
            .Where(p => p.TenantId == tenantId && !p.IsDeleted);

        if (invoiceId.HasValue)
            query = query.Where(p => p.InvoiceId == invoiceId.Value);

        if (!string.IsNullOrEmpty(status))
            query = query.Where(p => p.Status.ToString() == status);

        if (!string.IsNullOrEmpty(paymentMethod))
            query = query.Where(p => p.PaymentMethodType.ToString() == paymentMethod);

        if (fromDate.HasValue)
            query = query.Where(p => p.CreatedAt >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(p => p.CreatedAt <= toDate.Value);

        var totalCount = await query.CountAsync(cancellationToken);
        
        var items = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Include(p => p.Refunds)
            .ToListAsync(cancellationToken);

        return new PagedResult<Payment>(items, totalCount, page, pageSize);
    }

    public async Task<IEnumerable<Payment>> GetByInvoiceIdAsync(
        Guid invoiceId, CancellationToken cancellationToken = default)
    {
        return await _context.Payments
            .Where(p => p.InvoiceId == invoiceId && !p.IsDeleted)
            .Include(p => p.Refunds)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Payment>> GetByTenantAsync(
        Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await _context.Payments
            .Where(p => p.TenantId == tenantId && !p.IsDeleted)
            .Include(p => p.Refunds)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Payment?> GetByExternalIdAsync(
        string externalPaymentId, CancellationToken cancellationToken = default)
    {
        return await _context.Payments
            .Include(p => p.Refunds)
            .FirstOrDefaultAsync(p => p.ExternalPaymentId == externalPaymentId && !p.IsDeleted, cancellationToken);
    }

    public async Task AddAsync(Payment payment, CancellationToken cancellationToken = default)
    {
        _context.Payments.Add(payment);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Payment payment, CancellationToken cancellationToken = default)
    {
        _context.Payments.Update(payment);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, Guid deletedBy, CancellationToken cancellationToken = default)
    {
        var payment = await GetByIdAsync(id, cancellationToken);
        if (payment != null)
        {
            payment.SoftDelete(deletedBy);
            await UpdateAsync(payment, cancellationToken);
        }
    }

    public async Task<PaymentStatisticsDto> GetStatisticsAsync(
        Guid tenantId, 
        DateTime? fromDate, 
        DateTime? toDate, 
        CancellationToken cancellationToken = default)
    {
        var query = _context.Payments
            .Where(p => p.TenantId == tenantId && !p.IsDeleted);

        if (fromDate.HasValue)
            query = query.Where(p => p.CreatedAt >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(p => p.CreatedAt <= toDate.Value);

        var payments = await query.ToListAsync(cancellationToken);

        var totalPayments = payments.Count;
        var totalAmount = payments.Sum(p => p.Amount);
        var completedAmount = payments.Where(p => p.Status == Domain.Enums.PaymentStatus.Completed).Sum(p => p.Amount);
        var pendingAmount = payments.Where(p => p.Status == Domain.Enums.PaymentStatus.Pending).Sum(p => p.Amount);
        var failedAmount = payments.Where(p => p.Status == Domain.Enums.PaymentStatus.Failed).Sum(p => p.Amount);

        var statusCounts = payments.GroupBy(p => p.Status.ToString())
            .ToDictionary(g => g.Key, g => g.Count());

        var monthlyTotals = payments.GroupBy(p => new { p.CreatedAt.Year, p.CreatedAt.Month })
            .ToDictionary(
                g => $"{g.Key.Year}-{g.Key.Month:00}",
                g => g.Sum(p => p.Amount));

        return new PaymentStatisticsDto(
            totalPayments,
            totalAmount,
            completedAmount,
            pendingAmount,
            failedAmount,
            statusCounts,
            monthlyTotals);
    }
}
