namespace PizzaSaga.SharedKernel.Domain;

/// <summary>
/// Базовый класс для агрегатов с идентификатором.
/// </summary>
public abstract class AggregateRootWithId : AggregateRoot, IAggregateRootWithId
{
    public Guid Id { get; protected set; }
}