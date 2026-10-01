using MassTransit;
using Order.Application.Abstractions.Messaging;

namespace Order.Infrastructure.Messaging;

/// <summary>
/// Реализация IIntegrationEventPublisher через MassTransit IPublishEndpoint.
/// </summary>
public sealed class MassTransitIntegrationEventPublisher(IPublishEndpoint publishEndpoint) : IIntegrationEventPublisher
{
    private readonly IPublishEndpoint _publishEndpoint = publishEndpoint;

    /// <inheritdoc />
    public Task Publish<T>(T integrationEvent, CancellationToken cancellationToken) where T : class
        => _publishEndpoint.Publish(integrationEvent, cancellationToken);
}
