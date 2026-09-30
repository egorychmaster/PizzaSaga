using PizzaSaga.SharedKernel.Domain;

namespace Order.Application.Abstractions.DomainEvents;

/// <summary>
/// Предоставляет доступ к доменным событиям агрегатов, отслеживаемых текущим persistence-контекстом.
/// </summary>
public interface IDomainEventAccessor
{
    /// <summary>
    /// Возвращает все доменные события агрегатов, зарегистрированные в текущем persistence-контексте.
    /// </summary>
    IReadOnlyCollection<IDomainEvent> GetDomainEvents();

    /// <summary>
    /// Очищает доменные события агрегатов после их успешной обработки.
    /// </summary>
    void ClearDomainEvents();
}
