using Mediator;
using Order.Application.Abstractions.DomainEvents;
using Order.Application.Features.Orders.DomainEvents;
using Order.Domain.AggregatesModel.Orders.Events;
using PizzaSaga.SharedKernel.Domain;

namespace Order.Infrastructure.Persistence.DomainEvents;

/// <summary>
/// Диспетчер (маршрутизатор) доменных событий на основе локального Mediator.
/// Адаптирует доменные события к типизированным уведомлениям и публикует их через IMediator.
/// </summary>
public sealed class EfCoreDomainEventDispatcher : IDomainEventDispatcher
{
    private readonly IMediator _mediator;

    public EfCoreDomainEventDispatcher(IMediator mediator)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    /// <inheritdoc />
    public async Task DispatchAsync(IReadOnlyCollection<IDomainEvent> domainEvents, CancellationToken cancellationToken)
    {
        //  Единственная задача: преобразовать доменное событие в формат, который понимает Mediator, и передать дальше.
        foreach (var domainEvent in domainEvents)
        {
            switch (domainEvent)
            {
                case OrderCreatedDomainEvent orderCreatedEvent:
                    await _mediator.Publish(new OrderCreatedDomainEventNotification(orderCreatedEvent), cancellationToken);
                    break;

                default:
                    throw new InvalidOperationException(
                        $"Unsupported domain event type: {domainEvent.GetType().Name}");
            }
        }
    }
}
