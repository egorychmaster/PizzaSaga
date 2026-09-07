using Order.Domain.AggregatesModel.Orders.ValueObjects;
using PizzaSaga.SharedKernel.Domain;

namespace Order.Domain.AggregatesModel.Orders.Events;

/// <summary>
/// Доменное событие, сигнализирующее о создании нового заказа.
/// </summary>
public sealed record OrderCreatedDomainEvent(
    Guid OrderId,
    CustomerIdentity CustomerId,
    Money TotalAmount,
    DateTimeOffset OccurredAt) : IDomainEvent;