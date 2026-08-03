namespace Order.Application.Abstractions.Persistence;

/// <summary>
/// Интерфейс Unit of Work — абстракция над транзакционной работой с БД.
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// Выполняет операцию в транзакции с автоматическим сохранением,
    /// фиксацией изменений и повторным выполнением при transient-ошибках БД.
    /// </summary>
    Task<TResponse> ExecuteInTransactionAsync<TResponse>(
        Func<CancellationToken, Task<TResponse>> action,
        CancellationToken cancellationToken = default);
}