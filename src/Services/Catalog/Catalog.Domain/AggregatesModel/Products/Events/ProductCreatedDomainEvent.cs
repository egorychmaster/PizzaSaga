using PizzaSaga.SharedKernel.Domain;

namespace Catalog.Domain.AggregatesModel.Products.Events;

/// <summary>
/// Доменное событие создания продукта.
/// </summary>
public sealed record ProductCreatedDomainEvent(
    Guid ProductId,
    string Name,
    string Description) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}