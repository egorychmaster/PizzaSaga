namespace Catalog.Application.Abstractions.Persistence;

public interface IUnitOfWork
{
    /// <summary>
    /// Сохраняет агрегат и добавляет доменные события в одной транзакции.
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}