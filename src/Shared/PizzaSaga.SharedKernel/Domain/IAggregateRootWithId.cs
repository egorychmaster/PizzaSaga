namespace PizzaSaga.SharedKernel.Domain;

/// <summary>
/// Маркерный интерфейс для агрегатов с идентификатором.
/// </summary>
public interface IAggregateRootWithId
{
    /// <summary>
    /// Идентификатор агрегата.
    /// </summary>
    Guid Id { get; }
}