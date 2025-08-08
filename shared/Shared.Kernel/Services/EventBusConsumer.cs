// Event Bus Consumer for Cross-Service Communication
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Shared.Kernel.Events;
using System.Text.Json;

namespace Shared.Kernel.Services;

public class EventBusConsumer : BackgroundService
{
    private readonly ServiceBusProcessor _processor;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<EventBusConsumer> _logger;

    public EventBusConsumer(
        ServiceBusProcessor processor,
        IServiceProvider serviceProvider,
        ILogger<EventBusConsumer> logger)
    {
        _processor = processor;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _processor.ProcessMessageAsync += ProcessMessageHandler;
        _processor.ProcessErrorAsync += ProcessErrorHandler;

        await _processor.StartProcessingAsync(stoppingToken);

        // Keep the service running until cancellation is requested
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }

        await _processor.StopProcessingAsync();
    }

    private async Task ProcessMessageHandler(ProcessMessageEventArgs args)
    {
        try
        {
            var eventType = args.Message.ApplicationProperties.GetValueOrDefault("EventType")?.ToString();
            var eventData = args.Message.Body.ToString();

            _logger.LogInformation("Processing event {EventType} with ID {MessageId}", eventType, args.Message.MessageId);

            using var scope = _serviceProvider.CreateScope();
            
            // Route event to appropriate handler based on event type
            switch (eventType)
            {
                case "DocumentUploadedEvent":
                    await HandleDocumentUploadedEvent(scope, eventData);
                    break;
                case "InvoiceCreatedEvent":
                    await HandleInvoiceCreatedEvent(scope, eventData);
                    break;
                case "PaymentProcessedEvent":
                    await HandlePaymentProcessedEvent(scope, eventData);
                    break;
                case "WorkflowCompletedEvent":
                    await HandleWorkflowCompletedEvent(scope, eventData);
                    break;
                case "UserRegisteredEvent":
                    await HandleUserRegisteredEvent(scope, eventData);
                    break;
                default:
                    _logger.LogWarning("Unknown event type: {EventType}", eventType);
                    break;
            }

            // Complete the message to remove it from the queue
            await args.CompleteMessageAsync(args.Message);
            
            _logger.LogInformation("Successfully processed event {EventType} with ID {MessageId}", eventType, args.Message.MessageId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing message {MessageId}", args.Message.MessageId);
            
            // Abandon the message so it can be reprocessed
            await args.AbandonMessageAsync(args.Message);
        }
    }

    private async Task HandleDocumentUploadedEvent(IServiceScope scope, string eventData)
    {
        var documentEvent = JsonSerializer.Deserialize<DocumentUploadedEvent>(eventData);
        if (documentEvent == null) return;

        // Example: Start workflow for document approval
        // var workflowService = scope.ServiceProvider.GetService<IWorkflowService>();
        // await workflowService?.StartDocumentApprovalWorkflowAsync(documentEvent);

        // Example: Send notification
        // var notificationService = scope.ServiceProvider.GetService<INotificationService>();
        // await notificationService?.SendDocumentUploadNotificationAsync(documentEvent);

        _logger.LogInformation("Handled DocumentUploadedEvent for document {DocumentId}", documentEvent.DocumentId);
    }

    private async Task HandleInvoiceCreatedEvent(IServiceScope scope, string eventData)
    {
        var invoiceEvent = JsonSerializer.Deserialize<InvoiceCreatedEvent>(eventData);
        if (invoiceEvent == null) return;

        // Example: Start payment workflow
        // var workflowService = scope.ServiceProvider.GetService<IWorkflowService>();
        // await workflowService?.StartInvoiceApprovalWorkflowAsync(invoiceEvent);

        // Example: Send invoice notification
        // var notificationService = scope.ServiceProvider.GetService<INotificationService>();
        // await notificationService?.SendInvoiceCreatedNotificationAsync(invoiceEvent);

        _logger.LogInformation("Handled InvoiceCreatedEvent for invoice {InvoiceId}", invoiceEvent.InvoiceId);
    }

    private async Task HandlePaymentProcessedEvent(IServiceScope scope, string eventData)
    {
        var paymentEvent = JsonSerializer.Deserialize<PaymentProcessedEvent>(eventData);
        if (paymentEvent == null) return;

        // Example: Update invoice status
        // var invoiceService = scope.ServiceProvider.GetService<IInvoiceService>();
        // await invoiceService?.UpdateInvoicePaymentStatusAsync(paymentEvent);

        // Example: Send payment confirmation
        // var notificationService = scope.ServiceProvider.GetService<INotificationService>();
        // await notificationService?.SendPaymentConfirmationAsync(paymentEvent);

        _logger.LogInformation("Handled PaymentProcessedEvent for payment {PaymentId}", paymentEvent.PaymentId);
    }

    private async Task HandleWorkflowCompletedEvent(IServiceScope scope, string eventData)
    {
        var workflowEvent = JsonSerializer.Deserialize<WorkflowCompletedEvent>(eventData);
        if (workflowEvent == null) return;

        // Example: Update document or invoice status based on workflow result
        // var workflowService = scope.ServiceProvider.GetService<IWorkflowService>();
        // await workflowService?.ProcessWorkflowCompletionAsync(workflowEvent);

        _logger.LogInformation("Handled WorkflowCompletedEvent for workflow {WorkflowInstanceId}", workflowEvent.WorkflowInstanceId);
    }

    private async Task HandleUserRegisteredEvent(IServiceScope scope, string eventData)
    {
        var userEvent = JsonSerializer.Deserialize<UserRegisteredEvent>(eventData);
        if (userEvent == null) return;

        // Example: Send welcome email
        // var notificationService = scope.ServiceProvider.GetService<INotificationService>();
        // await notificationService?.SendWelcomeEmailAsync(userEvent);

        _logger.LogInformation("Handled UserRegisteredEvent for user {UserId}", userEvent.UserId);
    }

    private Task ProcessErrorHandler(ProcessErrorEventArgs args)
    {
        _logger.LogError(args.Exception, "Error in Service Bus processor: {ErrorSource}", args.ErrorSource);
        return Task.CompletedTask;
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await _processor.StopProcessingAsync();
        await base.StopAsync(cancellationToken);
    }
}
