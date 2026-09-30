namespace Order.Application.Abstractions.Persistence.Outbox;

/// <summary>
/// Абстракция для записи интеграционных событий в Outbox без SaveChanges.
/// Используется внутри UnitOfWork для атомарной записи Outbox + Aggregate.
/// </summary>
public interface IOutboxWriter
{
    /// <summary>
    /// Добавляет интеграционное событие в Outbox в рамках текущей транзакции.
    /// Вызов SaveChanges происходит позже, внутри UnitOfWork.
    /// </summary>
    /// <typeparam name="TEvent">Тип интеграционного события.</typeparam>
    /// <param name="aggregateId">Идентификатор агрегата, к которому относится событие.</param>
    /// <param name="integrationEvent">Событие для сохранения.</param>
    void Add<TEvent>(Guid aggregateId, TEvent integrationEvent)
        where TEvent : class;
}
