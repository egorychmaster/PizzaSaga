namespace Order.Domain.Common;

/// <summary>
/// Маркерный интерфейс для доменных событий.
/// </summary>
public interface IDomainEvent
{
    DateTimeOffset OccurredAt { get; }
}