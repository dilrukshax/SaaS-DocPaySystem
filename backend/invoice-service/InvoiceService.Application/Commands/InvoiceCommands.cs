using MediatR;
using InvoiceService.Application.DTOs;

namespace InvoiceService.Application.Commands;

// Create Invoice Command
public record CreateInvoiceCommand(
    Guid DocumentId,
    Guid CustomerId,
    decimal Amount,
    string Currency,
    DateTime DueDate,
    List<InvoiceLineItemDto> LineItems,
    Guid TenantId,
    Guid CreatedBy) : IRequest<InvoiceDto>;

// Update Invoice Command
public record UpdateInvoiceCommand(
    Guid Id,
    string? Description,
    DateTime DueDate,
    List<InvoiceLineItemDto> LineItems,
    Guid UpdatedBy) : IRequest<InvoiceDto?>;

// Delete Invoice Command
public record DeleteInvoiceCommand(
    Guid Id,
    Guid DeletedBy) : IRequest;

// Send Invoice Command
public record SendInvoiceCommand(
    Guid Id,
    string RecipientEmail,
    Guid SentBy) : IRequest<bool>;

// Process Payment Command
public record ProcessPaymentCommand(
    Guid InvoiceId,
    decimal Amount,
    string PaymentMethod,
    string? Reference,
    Guid ProcessedBy) : IRequest<InvoicePaymentDto>;

// Update Invoice Status Command
public record UpdateInvoiceStatusCommand(
    Guid Id,
    string Status,
    Guid UpdatedBy) : IRequest<InvoiceDto?>;

// Generate Invoice from Template Command
public record GenerateInvoiceFromTemplateCommand(
    Guid TemplateId,
    Guid CustomerId,
    List<InvoiceLineItemDto> LineItems,
    DateTime DueDate,
    Guid TenantId,
    Guid CreatedBy) : IRequest<InvoiceDto>;
