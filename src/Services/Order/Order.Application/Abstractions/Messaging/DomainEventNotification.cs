using Mediator;
using PizzaSaga.SharedKernel.Domain;

namespace Order.Application.Abstractions.Messaging;

/// <summary>
/// Адаптер доменного события для локальной шины Mediator.
/// Позволяет публиковать IDomainEvent через IMediator без зависимости Domain от Mediator.
/// </summary>
public sealed record DomainEventNotification(IDomainEvent DomainEvent) : INotification;
