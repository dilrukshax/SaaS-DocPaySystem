using Azure.Messaging.ServiceBus;
using DocumentService.Application.Events;
using DocumentService.Application.Interfaces;
using System.Text.Json;

namespace DocumentService.Infrastructure.Services;

public class EventPublisher : IEventPublisher
{
    private readonly ServiceBusSender _serviceBusSender;

    public EventPublisher(ServiceBusSender serviceBusSender)
    {
        _serviceBusSender = serviceBusSender;
    }

    public async Task PublishAsync<T>(T @event, CancellationToken cancellationToken = default) 
        where T : class
    {
        var eventType = typeof(T).Name;
        var eventData = JsonSerializer.Serialize(@event);

        var message = new ServiceBusMessage(eventData)
        {
            Subject = eventType,
            ContentType = "application/json",
            MessageId = Guid.NewGuid().ToString(),
            CorrelationId = GetCorrelationId(@event)
        };

        // Add custom properties
        message.ApplicationProperties.Add("EventType", eventType);
        message.ApplicationProperties.Add("Timestamp", DateTime.UtcNow.ToString("O"));

        if (@event is DocumentUploadedEvent uploadedEvent)
        {
            message.ApplicationProperties.Add("TenantId", uploadedEvent.TenantId.ToString());
            message.ApplicationProperties.Add("DocumentId", uploadedEvent.DocumentId.ToString());
        }
        else if (@event is DocumentDeletedEvent deletedEvent)
        {
            message.ApplicationProperties.Add("TenantId", deletedEvent.TenantId.ToString());
            message.ApplicationProperties.Add("DocumentId", deletedEvent.DocumentId.ToString());
        }
        else if (@event is DocumentVersionAddedEvent versionEvent)
        {
            message.ApplicationProperties.Add("TenantId", versionEvent.TenantId.ToString());
            message.ApplicationProperties.Add("DocumentId", versionEvent.DocumentId.ToString());
        }
        else if (@event is OCRProcessedEvent ocrEvent)
        {
            message.ApplicationProperties.Add("TenantId", ocrEvent.TenantId.ToString());
            message.ApplicationProperties.Add("DocumentId", ocrEvent.DocumentId.ToString());
        }

        await _serviceBusSender.SendMessageAsync(message, cancellationToken);
    }

    public async Task PublishBatchAsync<T>(IEnumerable<T> events, CancellationToken cancellationToken = default) 
        where T : class
    {
        var messages = events.Select(e =>
        {
            var eventType = typeof(T).Name;
            var eventData = JsonSerializer.Serialize(e);

            var message = new ServiceBusMessage(eventData)
            {
                Subject = eventType,
                ContentType = "application/json",
                MessageId = Guid.NewGuid().ToString(),
                CorrelationId = GetCorrelationId(e)
            };

            message.ApplicationProperties.Add("EventType", eventType);
            message.ApplicationProperties.Add("Timestamp", DateTime.UtcNow.ToString("O"));

            return message;
        }).ToList();

        if (messages.Any())
        {
            await _serviceBusSender.SendMessagesAsync(messages, cancellationToken);
        }
    }

    private static string GetCorrelationId<T>(T @event) where T : class
    {
        return @event switch
        {
            DocumentUploadedEvent uploadedEvent => uploadedEvent.DocumentId.ToString(),
            DocumentDeletedEvent deletedEvent => deletedEvent.DocumentId.ToString(),
            DocumentVersionAddedEvent versionEvent => versionEvent.DocumentId.ToString(),
            OCRProcessedEvent ocrEvent => ocrEvent.DocumentId.ToString(),
            _ => Guid.NewGuid().ToString()
        };
    }
}
