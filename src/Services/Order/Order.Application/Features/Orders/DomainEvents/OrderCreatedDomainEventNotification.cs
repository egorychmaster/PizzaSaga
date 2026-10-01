using Mediator;
using Order.Domain.AggregatesModel.Orders.Events;

namespace Order.Application.Features.Orders.DomainEvents;

/// <summary>
/// Уведомление для публикации доменного события OrderCreatedDomainEvent через Mediator.
/// </summary>
/// <param name="DomainEvent">Событие, которое будет опубликовано.</param>
public sealed record OrderCreatedDomainEventNotification(OrderCreatedDomainEvent DomainEvent) : INotification;
