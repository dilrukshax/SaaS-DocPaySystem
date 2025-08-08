using Azure.Messaging.ServiceBus;
using PaymentService.Application.Interfaces;
using Shared.Kernel.Events;
using System.Text.Json;

namespace PaymentService.Infrastructure.Services;

public class EventPublisher : IEventPublisher
{
    private readonly ServiceBusSender _serviceBusSender;
    private readonly ILogger<EventPublisher> _logger;

    public EventPublisher(ServiceBusSender serviceBusSender, ILogger<EventPublisher> logger)
    {
        _serviceBusSender = serviceBusSender;
        _logger = logger;
    }

    public async Task PublishAsync<T>(T @event, CancellationToken cancellationToken = default) where T : class
    {
        try
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

            // Add domain-specific properties
            if (@event is DomainEvent domainEvent)
            {
                message.ApplicationProperties.Add("TenantId", domainEvent.TenantId.ToString());
                message.ApplicationProperties.Add("AggregateId", domainEvent.AggregateId.ToString());
            }

            await _serviceBusSender.SendMessageAsync(message, cancellationToken);
            _logger.LogInformation("Published event {EventType} with ID {MessageId}", eventType, message.MessageId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish event {EventType}", typeof(T).Name);
            throw;
        }
    }

    public async Task PublishBatchAsync<T>(IEnumerable<T> events, CancellationToken cancellationToken = default) where T : class
    {
        try
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

                if (e is DomainEvent domainEvent)
                {
                    message.ApplicationProperties.Add("TenantId", domainEvent.TenantId.ToString());
                    message.ApplicationProperties.Add("AggregateId", domainEvent.AggregateId.ToString());
                }

                return message;
            }).ToList();

            if (messages.Any())
            {
                await _serviceBusSender.SendMessagesAsync(messages, cancellationToken);
                _logger.LogInformation("Published batch of {Count} events of type {EventType}", messages.Count, typeof(T).Name);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish batch events {EventType}", typeof(T).Name);
            throw;
        }
    }

    private static string GetCorrelationId<T>(T @event) where T : class
    {
        return @event switch
        {
            DomainEvent domainEvent => domainEvent.AggregateId.ToString(),
            _ => Guid.NewGuid().ToString()
        };
    }
}
