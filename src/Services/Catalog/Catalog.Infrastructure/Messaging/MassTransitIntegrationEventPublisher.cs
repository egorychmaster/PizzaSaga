using Catalog.Application.Abstractions.Messaging;
using MassTransit;

namespace Catalog.Infrastructure.Messaging;

/// <summary>
/// Реализация IIntegrationEventPublisher через MassTransit IPublishEndpoint.
/// Публикация происходит через EF Core Outbox — событие сохраняется в БД внутри той же транзакции, что и агрегат, 
/// а затем MassTransit отправляет его в RabbitMQ.
/// </summary>
public sealed class MassTransitIntegrationEventPublisher(IPublishEndpoint publishEndpoint) : IIntegrationEventPublisher
{
    private readonly IPublishEndpoint _publishEndpoint = publishEndpoint;

    /// <inheritdoc />
    public Task Publish<T>(T integrationEvent, CancellationToken cancellationToken) where T : class
        => _publishEndpoint.Publish(integrationEvent, cancellationToken);
}
