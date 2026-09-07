namespace PizzaSaga.SharedKernel.Domain;

/// <summary>
/// Базовый класс для всех корней агрегатов (Aggregate Root). Хранит доменные события.
/// </summary>
/// <remarks>
/// Предоставляет механизм регистрации доменных событий, возникающих в процессе изменения состояния агрегата.
/// После успешного сохранения агрегата инфраструктурный слой может извлечь накопленные события и опубликовать их через Transactional Outbox или иной механизм доставки.
/// </remarks>
public abstract class AggregateRoot
{
    private readonly List<IDomainEvent> _domainEvents = [];

    /// <summary>
    /// Список неопубликованных доменных событий.
    /// </summary>
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>
    /// Добавляет доменное событие в список.
    /// </summary>
    protected void AddDomainEvent(IDomainEvent domainEvent)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);

        _domainEvents.Add(domainEvent);
    }

    /// <summary>
    /// Очищает список доменных событий (обычно после публикации).
    /// </summary>
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}