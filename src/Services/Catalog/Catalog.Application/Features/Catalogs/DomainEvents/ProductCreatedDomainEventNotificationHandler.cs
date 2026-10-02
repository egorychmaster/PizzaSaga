using Catalog.Application.Abstractions.Messaging;
using Mediator;
using PizzaSaga.Contracts.Catalogs.IntegrationEvents;

namespace Catalog.Application.Features.Catalogs.DomainEvents;

/// <summary>
/// Обработчик доменного события создания продукта.
/// Преобразует внутреннее доменное событие во внешнее интеграционное событие
/// и публикует его через IIntegrationEventPublisher (MassTransit EF Core Outbox).
/// </summary>
public sealed class ProductCreatedDomainEventNotificationHandler(
    IIntegrationEventPublisher integrationEventPublisher) : INotificationHandler<ProductCreatedDomainEventNotification>
{
    private readonly IIntegrationEventPublisher _integrationEventPublisher = integrationEventPublisher;

    /// <inheritdoc />
    public async ValueTask Handle(ProductCreatedDomainEventNotification notification, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(notification);

        var domainEvent = notification.DomainEvent;

        // Преобразование доменного события в интеграционный контракт.
        var integrationEvent = new ProductCreatedIntegrationEvent(
            ProductId: domainEvent.ProductId,
            Name: domainEvent.Name,
            Description: domainEvent.Description,
            PriceAmount: domainEvent.PriceAmount,
            CurrencyCode: domainEvent.CurrencyCode);

        // Публикация через абстракцию (не зависит от MassTransit).
        await _integrationEventPublisher.Publish(integrationEvent, cancellationToken);
    }
}
