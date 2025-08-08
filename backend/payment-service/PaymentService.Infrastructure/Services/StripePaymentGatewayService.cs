using PaymentService.Application.Interfaces;
using PaymentService.Domain.Enums;
using Stripe;
using Stripe.Checkout;

namespace PaymentService.Infrastructure.Services;

public class StripePaymentGatewayService : IPaymentGatewayService
{
    private readonly StripeClient _stripeClient;
    private readonly ILogger<StripePaymentGatewayService> _logger;

    public StripePaymentGatewayService(StripeClient stripeClient, ILogger<StripePaymentGatewayService> logger)
    {
        _stripeClient = stripeClient;
        _logger = logger;
    }

    public async Task<PaymentGatewayResult> ProcessPaymentAsync(
        decimal amount,
        string currency,
        string paymentMethod,
        Dictionary<string, object> metadata,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var options = new PaymentIntentCreateOptions
            {
                Amount = (long)(amount * 100), // Stripe uses cents
                Currency = currency.ToLowerInvariant(),
                PaymentMethod = paymentMethod,
                ConfirmationMethod = "manual",
                Confirm = true,
                Metadata = metadata.ToDictionary(kvp => kvp.Key, kvp => kvp.Value?.ToString() ?? "")
            };

            var service = new PaymentIntentService(_stripeClient);
            var paymentIntent = await service.CreateAsync(options, cancellationToken: cancellationToken);

            return new PaymentGatewayResult(
                IsSuccess: paymentIntent.Status == "succeeded",
                PaymentReference: paymentIntent.Id,
                Status: MapStripeStatus(paymentIntent.Status),
                Amount: amount,
                Currency: currency,
                ErrorMessage: paymentIntent.Status != "succeeded" ? paymentIntent.LastPaymentError?.Message : null,
                ProcessedAt: DateTime.UtcNow,
                GatewayData: new Dictionary<string, object>
                {
                    ["stripe_payment_intent_id"] = paymentIntent.Id,
                    ["stripe_status"] = paymentIntent.Status,
                    ["stripe_client_secret"] = paymentIntent.ClientSecret ?? ""
                });
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe payment processing failed for amount {Amount} {Currency}", amount, currency);
            return new PaymentGatewayResult(
                IsSuccess: false,
                PaymentReference: null,
                Status: PaymentStatus.Failed,
                Amount: amount,
                Currency: currency,
                ErrorMessage: ex.Message,
                ProcessedAt: DateTime.UtcNow,
                GatewayData: new Dictionary<string, object>
                {
                    ["stripe_error_code"] = ex.StripeError.Code ?? "",
                    ["stripe_error_type"] = ex.StripeError.Type ?? ""
                });
        }
    }

    public async Task<PaymentGatewayResult> RefundPaymentAsync(
        string paymentReference,
        decimal amount,
        string currency,
        string? reason = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var options = new RefundCreateOptions
            {
                PaymentIntent = paymentReference,
                Amount = (long)(amount * 100), // Stripe uses cents
                Reason = reason switch
                {
                    "duplicate" => "duplicate",
                    "fraudulent" => "fraudulent",
                    "requested_by_customer" => "requested_by_customer",
                    _ => "requested_by_customer"
                }
            };

            var service = new RefundService(_stripeClient);
            var refund = await service.CreateAsync(options, cancellationToken: cancellationToken);

            return new PaymentGatewayResult(
                IsSuccess: refund.Status == "succeeded",
                PaymentReference: refund.Id,
                Status: refund.Status == "succeeded" ? PaymentStatus.Refunded : PaymentStatus.Failed,
                Amount: amount,
                Currency: currency,
                ErrorMessage: refund.Status != "succeeded" ? refund.FailureReason : null,
                ProcessedAt: DateTime.UtcNow,
                GatewayData: new Dictionary<string, object>
                {
                    ["stripe_refund_id"] = refund.Id,
                    ["stripe_status"] = refund.Status,
                    ["original_payment_intent"] = paymentReference
                });
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe refund processing failed for payment {PaymentReference}", paymentReference);
            return new PaymentGatewayResult(
                IsSuccess: false,
                PaymentReference: null,
                Status: PaymentStatus.Failed,
                Amount: amount,
                Currency: currency,
                ErrorMessage: ex.Message,
                ProcessedAt: DateTime.UtcNow,
                GatewayData: new Dictionary<string, object>
                {
                    ["stripe_error_code"] = ex.StripeError.Code ?? "",
                    ["stripe_error_type"] = ex.StripeError.Type ?? ""
                });
        }
    }

    public async Task<string> CreatePaymentSessionAsync(
        decimal amount,
        string currency,
        string successUrl,
        string cancelUrl,
        Dictionary<string, object> metadata,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            UnitAmount = (long)(amount * 100),
                            Currency = currency.ToLowerInvariant(),
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = "Payment",
                            },
                        },
                        Quantity = 1,
                    },
                },
                Mode = "payment",
                SuccessUrl = successUrl,
                CancelUrl = cancelUrl,
                Metadata = metadata.ToDictionary(kvp => kvp.Key, kvp => kvp.Value?.ToString() ?? "")
            };

            var service = new SessionService(_stripeClient);
            var session = await service.CreateAsync(options, cancellationToken: cancellationToken);

            return session.Url;
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Failed to create Stripe checkout session");
            throw new InvalidOperationException($"Failed to create payment session: {ex.Message}", ex);
        }
    }

    public async Task<PaymentGatewayResult> GetPaymentStatusAsync(
        string paymentReference,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var service = new PaymentIntentService(_stripeClient);
            var paymentIntent = await service.GetAsync(paymentReference, cancellationToken: cancellationToken);

            return new PaymentGatewayResult(
                IsSuccess: paymentIntent.Status == "succeeded",
                PaymentReference: paymentIntent.Id,
                Status: MapStripeStatus(paymentIntent.Status),
                Amount: paymentIntent.Amount / 100m,
                Currency: paymentIntent.Currency.ToUpperInvariant(),
                ErrorMessage: paymentIntent.Status != "succeeded" ? paymentIntent.LastPaymentError?.Message : null,
                ProcessedAt: DateTime.UtcNow,
                GatewayData: new Dictionary<string, object>
                {
                    ["stripe_payment_intent_id"] = paymentIntent.Id,
                    ["stripe_status"] = paymentIntent.Status
                });
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Failed to get Stripe payment status for {PaymentReference}", paymentReference);
            throw new InvalidOperationException($"Failed to get payment status: {ex.Message}", ex);
        }
    }

    private static PaymentStatus MapStripeStatus(string stripeStatus) => stripeStatus switch
    {
        "requires_payment_method" => PaymentStatus.Pending,
        "requires_confirmation" => PaymentStatus.Pending,
        "requires_action" => PaymentStatus.Processing,
        "processing" => PaymentStatus.Processing,
        "succeeded" => PaymentStatus.Completed,
        "canceled" => PaymentStatus.Cancelled,
        _ => PaymentStatus.Failed
    };
}
