using Catalog.Application.Abstractions.Persistence;
using PizzaSaga.SharedKernel.Domain.DomainEvents;

namespace Catalog.Infrastructure.Persistence;

/// <summary>
/// Реализация IUnitOfWork для EF Core + PostgreSQL.
/// Доменные события агрегатов диспетчеризируются через Mediator в Application-слой,
/// где они преобразуются в интеграционные события и публикуются через MassTransit EF Core Outbox.
/// </summary>
public sealed class UnitOfWork : IUnitOfWork
{
    private readonly CatalogDbContext _context;
    private readonly IDomainEventAccessor _domainEventAccessor;
    private readonly IDomainEventDispatcher _domainEventDispatcher;

    public UnitOfWork(
        CatalogDbContext context,
        IDomainEventAccessor domainEventAccessor,
        IDomainEventDispatcher domainEventDispatcher)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _domainEventAccessor = domainEventAccessor ?? throw new ArgumentNullException(nameof(domainEventAccessor));
        _domainEventDispatcher = domainEventDispatcher ?? throw new ArgumentNullException(nameof(domainEventDispatcher));
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // 1. Извлекаем Domain Events из агрегатов, отслеживаемых текущим DbContext.
        var domainEvents = _domainEventAccessor.GetDomainEvents();
        if (domainEvents.Count > 0)
        {
            // Преобразуем Domain Events в локальные действия Application.
            // В случае OrderCreatedDomainEvent это приведёт к добавлению OrderCreatedIntegrationEvent в Outbox.
            await _domainEventDispatcher.DispatchAsync(domainEvents, cancellationToken);

            // События больше не должны повторно обрабатываться в рамках этого DbContext.
            _domainEventAccessor.ClearDomainEvents();
        }

        // 2. Сохраняем изменения агрегатов в БД (включая Outbox-сообщения MassTransit).
        return await _context.SaveChangesAsync(cancellationToken);
    }
}