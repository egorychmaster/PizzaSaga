using Mediator;
using Order.Domain.AggregatesModel.Orders.Events;

namespace Order.Application.Features.Orders.DomainEvents;

/// <summary>
/// Уведомление Mediator о создании заказа.
/// </summary>
public sealed record OrderCreatedDomainEventNotification(OrderCreatedDomainEvent DomainEvent) : INotification;