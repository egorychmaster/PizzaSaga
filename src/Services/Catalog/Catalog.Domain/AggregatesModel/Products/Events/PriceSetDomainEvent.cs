using Catalog.Domain.AggregatesModel.Products.ValueObjects;
using PizzaSaga.SharedKernel.Domain;

namespace Catalog.Domain.AggregatesModel.Products.Events;

/// <summary>
/// Доменное событие установки цены.
/// </summary>
public sealed record PriceSetDomainEvent(
    Guid ProductId,
    Price Price) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}