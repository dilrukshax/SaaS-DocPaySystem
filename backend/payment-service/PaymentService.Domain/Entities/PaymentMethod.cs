using PaymentService.Domain.ValueObjects;

namespace PaymentService.Domain.Entities;

public class PaymentMethod
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public PaymentMethodType Type { get; private set; }
    public string? CardLast4 { get; private set; }
    public string? CardBrand { get; private set; }
    public string? ExpiryMonth { get; private set; }
    public string? ExpiryYear { get; private set; }
    public string? BankName { get; private set; }
    public string? AccountNumberLast4 { get; private set; }
    public string? PayPalEmail { get; private set; }
    public string GatewayMethodId { get; private set; }
    public PaymentGateway Gateway { get; private set; }
    public bool IsDefault { get; private set; }
    public bool IsActive { get; private set; }
    public Guid UserId { get; private set; }
    public Guid TenantId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private readonly List<Payment> _payments = new();
    public IReadOnlyList<Payment> Payments => _payments.AsReadOnly();

    private PaymentMethod() { } // For EF Core

    public PaymentMethod(string name, PaymentMethodType type, string gatewayMethodId, 
        PaymentGateway gateway, Guid userId, Guid tenantId)
    {
        Id = Guid.NewGuid();
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Type = type;
        GatewayMethodId = gatewayMethodId ?? throw new ArgumentNullException(nameof(gatewayMethodId));
        Gateway = gateway;
        UserId = userId;
        TenantId = tenantId;
        IsDefault = false;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
    }

    public void SetCardDetails(string cardLast4, string cardBrand, string expiryMonth, string expiryYear)
    {
        if (Type != PaymentMethodType.CreditCard && Type != PaymentMethodType.DebitCard)
            throw new InvalidOperationException("Card details can only be set for card payment methods");

        CardLast4 = cardLast4;
        CardBrand = cardBrand;
        ExpiryMonth = expiryMonth;
        ExpiryYear = expiryYear;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetBankDetails(string bankName, string accountNumberLast4)
    {
        if (Type != PaymentMethodType.BankTransfer)
            throw new InvalidOperationException("Bank details can only be set for bank transfer payment methods");

        BankName = bankName;
        AccountNumberLast4 = accountNumberLast4;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetPayPalDetails(string paypalEmail)
    {
        if (Type != PaymentMethodType.PayPal)
            throw new InvalidOperationException("PayPal details can only be set for PayPal payment methods");

        PayPalEmail = paypalEmail;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetAsDefault()
    {
        IsDefault = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void RemoveAsDefault()
    {
        IsDefault = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        IsDefault = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }
}
