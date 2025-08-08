using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaymentService.Application.Commands;
using PaymentService.Application.DTOs;
using PaymentService.Application.Queries;
using Shared.Kernel.Constants;
using Shared.Kernel.Middleware;

namespace PaymentService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PaymentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public PaymentsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Process a new payment
    /// </summary>
    [HttpPost]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Manager},{Roles.User}")]
    public async Task<ActionResult<PaymentDto>> ProcessPayment([FromBody] ProcessPaymentRequest request)
    {
        var tenantId = HttpContext.GetTenantId();
        var userId = HttpContext.GetUserId();

        if (!tenantId.HasValue || !userId.HasValue)
            return BadRequest("Invalid tenant or user context");

        var command = new ProcessPaymentCommand(
            request.InvoiceId,
            request.Amount,
            request.Currency,
            request.PaymentMethodType,
            request.PaymentMethodId,
            request.Reference,
            request.Metadata ?? new Dictionary<string, object>(),
            tenantId.Value,
            userId.Value);

        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetPayment), new { id = result.Id }, result.ToSuccessResponse());
    }

    /// <summary>
    /// Get payments with pagination and filtering
    /// </summary>
    [HttpGet]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Manager},{Roles.Viewer}")]
    public async Task<ActionResult<PagedResult<PaymentDto>>> GetPayments(
        [FromQuery] Guid? invoiceId = null,
        [FromQuery] string? status = null,
        [FromQuery] string? paymentMethod = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var tenantId = HttpContext.GetTenantId();
        if (!tenantId.HasValue)
            return BadRequest("Invalid tenant context");

        var query = new GetPaymentsQuery(
            tenantId.Value, invoiceId, status, paymentMethod, fromDate, toDate, page, pageSize);

        var result = await _mediator.Send(query);
        return Ok(result.ToSuccessResponse());
    }

    /// <summary>
    /// Get a specific payment by ID
    /// </summary>
    [HttpGet("{id}")]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Manager},{Roles.Viewer}")]
    public async Task<ActionResult<PaymentDto>> GetPayment(Guid id)
    {
        var query = new GetPaymentQuery(id);
        var result = await _mediator.Send(query);
        
        if (result == null)
            return NotFound();

        return Ok(result.ToSuccessResponse());
    }

    /// <summary>
    /// Get payments for a specific invoice
    /// </summary>
    [HttpGet("invoice/{invoiceId}")]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Manager},{Roles.Viewer}")]
    public async Task<ActionResult<IEnumerable<PaymentDto>>> GetInvoicePayments(Guid invoiceId)
    {
        var query = new GetInvoicePaymentsQuery(invoiceId);
        var result = await _mediator.Send(query);
        return Ok(result.ToSuccessResponse());
    }

    /// <summary>
    /// Refund a payment
    /// </summary>
    [HttpPost("{id}/refund")]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Manager}")]
    public async Task<ActionResult<RefundDto>> RefundPayment(
        Guid id,
        [FromBody] RefundPaymentRequest request)
    {
        var tenantId = HttpContext.GetTenantId();
        var userId = HttpContext.GetUserId();

        if (!tenantId.HasValue || !userId.HasValue)
            return BadRequest("Invalid tenant or user context");

        var command = new RefundPaymentCommand(
            id, request.Amount, request.Reason, userId.Value);

        var result = await _mediator.Send(command);
        
        if (result == null)
            return NotFound();

        return Ok(result.ToSuccessResponse());
    }

    /// <summary>
    /// Update payment status (for webhook processing)
    /// </summary>
    [HttpPut("{id}/status")]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Manager}")]
    public async Task<ActionResult<PaymentDto>> UpdatePaymentStatus(
        Guid id,
        [FromBody] UpdatePaymentStatusRequest request)
    {
        var userId = HttpContext.GetUserId();
        if (!userId.HasValue)
            return BadRequest("Invalid user context");

        var command = new UpdatePaymentStatusCommand(
            id, request.Status, request.ExternalPaymentId, request.Metadata, userId.Value);

        var result = await _mediator.Send(command);
        
        if (result == null)
            return NotFound();

        return Ok(result.ToSuccessResponse());
    }

    /// <summary>
    /// Get payment statistics
    /// </summary>
    [HttpGet("statistics")]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Manager}")]
    public async Task<ActionResult<PaymentStatisticsDto>> GetPaymentStatistics(
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
        var tenantId = HttpContext.GetTenantId();
        if (!tenantId.HasValue)
            return BadRequest("Invalid tenant context");

        var query = new GetPaymentStatisticsQuery(tenantId.Value, fromDate, toDate);
        var result = await _mediator.Send(query);
        return Ok(result.ToSuccessResponse());
    }

    /// <summary>
    /// Create payment session (for frontend checkout)
    /// </summary>
    [HttpPost("sessions")]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Manager},{Roles.User}")]
    public async Task<ActionResult<PaymentSessionDto>> CreatePaymentSession(
        [FromBody] CreatePaymentSessionRequest request)
    {
        var tenantId = HttpContext.GetTenantId();
        var userId = HttpContext.GetUserId();

        if (!tenantId.HasValue || !userId.HasValue)
            return BadRequest("Invalid tenant or user context");

        var command = new CreatePaymentSessionCommand(
            request.InvoiceId,
            request.Amount,
            request.Currency,
            request.SuccessUrl,
            request.CancelUrl,
            request.Metadata ?? new Dictionary<string, object>(),
            tenantId.Value,
            userId.Value);

        var result = await _mediator.Send(command);
        return Ok(result.ToSuccessResponse());
    }
}

// Request DTOs
public record ProcessPaymentRequest(
    Guid InvoiceId,
    decimal Amount,
    string Currency,
    string PaymentMethodType,
    string? PaymentMethodId,
    string? Reference,
    Dictionary<string, object>? Metadata);

public record RefundPaymentRequest(
    decimal Amount,
    string? Reason);

public record UpdatePaymentStatusRequest(
    string Status,
    string? ExternalPaymentId,
    Dictionary<string, object>? Metadata);

public record CreatePaymentSessionRequest(
    Guid InvoiceId,
    decimal Amount,
    string Currency,
    string SuccessUrl,
    string CancelUrl,
    Dictionary<string, object>? Metadata);
