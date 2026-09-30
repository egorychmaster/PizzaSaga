using Mediator;
using Order.Application.Abstractions.DomainEvents;
using Order.Application.Abstractions.Messaging;
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
            INotification notification = new DomainEventNotification(domainEvent);

            await _mediator.Publish(notification, cancellationToken);
        }
    }
}
