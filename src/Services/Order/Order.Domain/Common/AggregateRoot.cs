namespace Order.Domain.Common;

/// <summary>
/// Базовый класс для всех корней агрегатов (Aggregate Root).
/// </summary>
/// <remarks>
/// Предоставляет механизм регистрации доменных событий, возникающих в процессе изменения состояния агрегата.
///
/// После успешного сохранения агрегата инфраструктурный слой может извлечь накопленные события и опубликовать их через Transactional Outbox или иной механизм доставки.
/// </remarks>
public abstract class AggregateRoot
{
    private readonly List<IDomainEvent> _domainEvents = [];

    /// <summary>
    /// Коллекция доменных событий, зарегистрированных агрегатом.
    /// </summary>
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>
    /// Регистрирует новое доменное событие.
    /// </summary>
    /// <param name="domainEvent">
    /// Доменное событие, описывающее произошедшее изменение состояния агрегата.
    /// </param>
    protected void RaiseDomainEvent(IDomainEvent domainEvent)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);

        _domainEvents.Add(domainEvent);
    }

    /// <summary>
    /// Удаляет все зарегистрированные доменные события.
    /// </summary>
    /// <remarks>
    /// Обычно вызывается инфраструктурным слоем после успешной публикации всех накопленных доменных событий.
    /// </remarks>
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}