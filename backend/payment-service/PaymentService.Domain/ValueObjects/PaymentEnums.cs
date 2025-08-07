namespace PaymentService.Domain.ValueObjects;

public enum PaymentStatus
{
    Pending = 0,
    Processing = 1,
    Completed = 2,
    Failed = 3,
    Cancelled = 4,
    Refunded = 5,
    PartiallyRefunded = 6,
    Disputed = 7
}

public enum PaymentMethodType
{
    CreditCard = 0,
    DebitCard = 1,
    BankTransfer = 2,
    PayPal = 3,
    DigitalWallet = 4,
    Cryptocurrency = 5
}

public enum PaymentGateway
{
    Stripe = 0,
    PayPal = 1,
    Square = 2,
    Authorize = 3,
    Braintree = 4
}
