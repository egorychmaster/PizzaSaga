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

    /// <summary>
    /// Сохраняет интеграционное событие в Outbox в рамках текущей транзакции.
    /// Используется для надёжной публикации событий через RabbitMQ.
    /// </summary>
    /// <typeparam name="TEvent">Тип интеграционного события.</typeparam>
    /// <param name="aggregateId">Идентификатор агрегата, к которому относится событие.</param>
    /// <param name="integrationEvent">Интеграционное событие для сохранения в Outbox.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    Task SaveWithOutboxAsync<TEvent>(
        Guid aggregateId,
        TEvent integrationEvent,
        CancellationToken cancellationToken = default)
        where TEvent : class;
}
