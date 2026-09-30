using Mediator;
using Order.Application.Abstractions.Persistence.Outbox;
using PizzaSaga.Contracts.Orders.IntegrationEvents;
using PizzaSaga.Contracts.Orders.Models;

namespace Order.Application.Features.Orders.DomainEvents;

/// <summary>
/// Обработчик доменного события создания заказа.
/// Handler определяет бизнес-логику. Является адаптером, изолирует mapping Domain → Integration.
/// Cодержит конкретное бизнес-сопоставление OrderCreatedDomainEvent с OrderCreatedIntegrationEvent и добавляет его в Outbox.
/// </summary>
public sealed class OrderCreatedDomainEventHandler(IOutboxWriter outboxWriter) : INotificationHandler<OrderCreatedDomainEventNotification>
{
    private readonly IOutboxWriter _outboxWriter = outboxWriter;

    /// <inheritdoc />
    public ValueTask Handle(OrderCreatedDomainEventNotification notification, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(notification);

        // Бизнес-правило: при создании заказа нужно уведомить внешние системы через Outbox.

        var domainEvent = notification.DomainEvent;

        var integrationEvent = new OrderCreatedIntegrationEvent(
            OrderId: domainEvent.OrderId,
            CustomerId: domainEvent.CustomerId.Value,
            TotalAmount: domainEvent.TotalAmount.Amount,
            CurrencyCode: domainEvent.TotalAmount.Currency.Code,
            CreatedAt: domainEvent.OccurredAt,
            Items: domainEvent.Items
                .Select(x => new PizzaItem(x.ProductId, x.Quantity))
                .ToArray());

        _outboxWriter.Add(domainEvent.OrderId, integrationEvent);

        return ValueTask.CompletedTask;
    }
}
