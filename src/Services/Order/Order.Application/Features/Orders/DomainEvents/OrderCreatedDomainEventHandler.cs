using Mediator;
using Order.Application.Abstractions.Messaging;
using Order.Application.Abstractions.Persistence.Outbox;
using Order.Domain.AggregatesModel.Orders.Events;
using PizzaSaga.Contracts.Orders.IntegrationEvents;
using PizzaSaga.Contracts.Orders.Models;

namespace Order.Application.Features.Orders.DomainEvents;

/// <summary>
/// Обработчик доменного события создания заказа.
/// Handler определяет бизнес-логику. Является адаптером, изолирует mapping Domain → Integration.
/// Cодержит конкретное бизнес-сопоставление OrderCreatedDomainEvent с OrderCreatedIntegrationEvent и добавляет его в Outbox.
/// </summary>
public sealed class OrderCreatedDomainEventHandler(IOutboxWriter outboxWriter) : INotificationHandler<DomainEventNotification>
{
    private readonly IOutboxWriter _outboxWriter = outboxWriter;

    /// <inheritdoc />
    public ValueTask Handle(DomainEventNotification notification, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(notification);

        // Бизнес-правило: при создании заказа нужно уведомить внешние системы через Outbox.

        var domainEvent = notification.DomainEvent;

        // Приведение к конкретному типу события — безопасно, так как обработчик вызывается только для OrderCreatedDomainEvent
        if (domainEvent is not OrderCreatedDomainEvent orderCreatedEvent)
        {
            return ValueTask.CompletedTask;
        }

        var integrationEvent = new OrderCreatedIntegrationEvent(
            OrderId: orderCreatedEvent.OrderId,
            CustomerId: orderCreatedEvent.CustomerId.Value,
            TotalAmount: orderCreatedEvent.TotalAmount.Amount,
            CurrencyCode: orderCreatedEvent.TotalAmount.Currency.Code,
            CreatedAt: orderCreatedEvent.OccurredAt,
            Items: orderCreatedEvent.Items
                .Select(x => new PizzaItem(x.ProductId, x.Quantity))
                .ToArray());

        _outboxWriter.Add(orderCreatedEvent.OrderId, integrationEvent);

        return ValueTask.CompletedTask;
    }
}
