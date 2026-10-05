using Mediator;
using PizzaSaga.Contracts.Orders.IntegrationEvents;
using PizzaSaga.Contracts.Orders.Models;
using PizzaSaga.SharedKernel.Messaging;

namespace Order.Application.Features.Orders.DomainEvents;

/// <summary>
/// Обработчик доменного события создания заказа.
/// Преобразует внутреннее доменное событие в внешний интеграционный контракт
/// и публикует его через IIntegrationEventPublisher.
/// </summary>
public sealed class OrderCreatedDomainEventNotificationHandler(IIntegrationEventPublisher integrationEventPublisher) : INotificationHandler<OrderCreatedDomainEventNotification>
{
    private readonly IIntegrationEventPublisher _integrationEventPublisher = integrationEventPublisher;

    /// <inheritdoc />
    public async ValueTask Handle(OrderCreatedDomainEventNotification notification, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(notification);

        var domainEvent = notification.DomainEvent;

        // Преобразование доменного события в интеграционный контракт.
        var integrationEvent = new OrderCreatedIntegrationEvent(
            OrderId: domainEvent.OrderId,
            CustomerId: domainEvent.CustomerId.Value,
            TotalAmount: domainEvent.TotalAmount.Amount,
            CurrencyCode: domainEvent.TotalAmount.Currency.Code,
            CreatedAt: domainEvent.OccurredAt,
            Items: domainEvent.Items
                .Select(x => new PizzaItem(x.ProductId, x.Quantity))
                .ToArray());

        // Публикация через абстракцию (не зависит от MassTransit).
        await _integrationEventPublisher.Publish(integrationEvent, cancellationToken);
    }
}
