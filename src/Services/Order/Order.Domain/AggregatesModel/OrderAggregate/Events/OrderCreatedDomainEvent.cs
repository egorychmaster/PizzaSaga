using Order.Domain.Common;
using Order.Domain.ValueObjects;

namespace Order.Domain.AggregatesModel.OrderAggregate.Events;

/// <summary>
/// Доменное событие, сигнализирующее о создании нового заказа.
/// </summary>
public sealed record OrderCreatedDomainEvent(
    Guid OrderId,
    CustomerIdentity CustomerId,
    Money TotalAmount,
    DateTimeOffset OccurredAt) : IDomainEvent;