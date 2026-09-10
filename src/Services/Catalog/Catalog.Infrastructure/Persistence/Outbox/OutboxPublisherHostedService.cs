using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PizzaSaga.Contracts.IntegrationEvents.Catalogs;
using System.Text.Json;

namespace Catalog.Infrastructure.Persistence.Outbox;

/// <summary>
/// Фоновый сервис, который публикует Outbox-сообщения в RabbitMQ через MassTransit.
/// Публикует интеграционные события.
/// </summary>
public sealed class OutboxPublisherHostedService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OutboxPublisherHostedService> _logger;

    public OutboxPublisherHostedService(
        IServiceProvider serviceProvider,
        ILogger<OutboxPublisherHostedService> logger)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting Outbox Publisher service.");
        _ = PublishLoop(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private async Task PublishLoop(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();

                // Получаем неопубликованные сообщения
                var messages = context.OutboxMessages
                    .Where(x => !x.IsPublished)
                    .Take(100)
                    .ToList();

                if (messages.Count == 0)
                {
                    await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
                    continue;
                }

                foreach (var message in messages)
                {
                    try
                    {
                        var eventType = Type.GetType(message.MessageType);
                        if (eventType is null)
                        {
                            _logger.LogWarning("Unknown message type: {MessageType}", message.MessageType);
                            await MarkAsPublishedAsync(context, message.Id, cancellationToken);
                            continue;
                        }

                        // Проверяем тип — публикуем только поддерживаемые интеграционные события
                        if (eventType != typeof(ProductCreatedIntegrationEvent))
                        {
                            _logger.LogWarning("Unsupported integration event type: {MessageType}", message.MessageType);
                            await MarkAsPublishedAsync(context, message.Id, cancellationToken);
                            continue;
                        }

                        // Десериализуем в ProductCreatedIntegrationEvent (из PizzaSaga.Contracts)
                        var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
                        var integrationEvent = JsonSerializer.Deserialize(message.Payload, eventType, jsonOptions) as ProductCreatedIntegrationEvent;
                        if (integrationEvent is null)
                        {
                            _logger.LogWarning("Failed to deserialize event: {Payload}", message.Payload);
                            await MarkAsPublishedAsync(context, message.Id, cancellationToken);
                            continue;
                        }

                        // Публикация через MassTransit
                        var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();
                        await publishEndpoint.Publish(integrationEvent, cancellationToken);

                        // Помечаем как опубликованное
                        await MarkAsPublishedAsync(context, message.Id, cancellationToken);

                        _logger.LogInformation("Outbox message {MessageId} published successfully.", message.Id);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to publish outbox message {MessageId}.", message.Id);
                        // Пропускаем и переходим к следующему сообщению
                    }
                }

                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Outbox publisher loop error.");
                await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
            }
        }
    }

    /// <summary>
    /// Помечает событие как опубликованное.
    /// </summary>
    private async Task MarkAsPublishedAsync(CatalogDbContext context, Guid messageId, CancellationToken ct)
    {
        var message = await context.OutboxMessages.FindAsync([messageId], ct);
        if (message is not null)
        {
            message.IsPublished = true;
            context.OutboxMessages.Update(message);
            await context.SaveChangesAsync(ct);
        }
    }
}