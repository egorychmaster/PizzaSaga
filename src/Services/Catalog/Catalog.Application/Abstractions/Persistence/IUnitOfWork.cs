using PizzaSaga.SharedKernel.Domain;

namespace Catalog.Application.Abstractions.Persistence;

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Сохраняет агрегат и добавляет доменные события в Outbox в одной транзакции.
    /// </summary>
    Task SaveWithOutboxAsync<TAggregate>(TAggregate aggregate, CancellationToken cancellationToken = default)
        where TAggregate : AggregateRootWithId;
}