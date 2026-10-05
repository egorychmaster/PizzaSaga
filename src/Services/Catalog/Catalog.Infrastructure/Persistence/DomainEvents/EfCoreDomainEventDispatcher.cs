using Catalog.Application.Features.Catalogs.DomainEvents;
using Catalog.Domain.AggregatesModel.Products.Events;
using Mediator;
using PizzaSaga.SharedKernel.Domain;
using PizzaSaga.SharedKernel.Domain.DomainEvents;

namespace Catalog.Infrastructure.Persistence.DomainEvents;

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
                case ProductCreatedDomainEvent productCreated:
                    await _mediator.Publish(new ProductCreatedDomainEventNotification(productCreated), cancellationToken);
                    break;

                default:
                    throw new InvalidOperationException($"Unsupported domain event type: {domainEvent.GetType().Name}");
            }
        }
    }
}