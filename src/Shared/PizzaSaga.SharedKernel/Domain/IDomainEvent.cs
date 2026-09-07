namespace PizzaSaga.SharedKernel.Domain;

/// <summary>
/// Маркерный интерфейс для доменных событий.
/// </summary>
public interface IDomainEvent
{
    /// <summary>
    /// Временная метка возникновения события.
    /// </summary>
    DateTimeOffset OccurredAt { get; }
}