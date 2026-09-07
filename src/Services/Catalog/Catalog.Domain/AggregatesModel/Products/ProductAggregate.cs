using Catalog.Domain.AggregatesModel.Products.Events;
using Catalog.Domain.AggregatesModel.Products.Exceptions.ProductAggregates;
using Catalog.Domain.AggregatesModel.Products.ValueObjects;
using PizzaSaga.SharedKernel.Domain;

namespace Catalog.Domain.AggregatesModel.Products;

/// <summary>
/// Агрегат продукта.
/// Управляет информацией о продукте и его текущей ценой.
/// </summary>
public sealed class ProductAggregate : AggregateRoot
{
    private Price? _currentPrice;

    public Guid Id { get; private set; }
    public string Name { get; private set; } = null!;
    public string Description { get; private set; } = null!;

    /// <summary>
    /// Текущая цена (одна активная запись).
    /// </summary>
    public Price? CurrentPrice => _currentPrice;

    // EF Core
    private ProductAggregate() { }

    public ProductAggregate(Guid id, string name, string description)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new InvalidProductNameException();

        Id = id;
        Name = name.Trim();
        Description = description?.Trim() ?? "";

        AddDomainEvent(new ProductCreatedDomainEvent(Id, Name, Description));
    }

    public static ProductAggregate Create(Guid id, string name, string description)
        => new(id, name, description);

    public void SetCurrentPrice(decimal amount, string currencyCode)
    {
        _currentPrice = Price.Create(amount, currencyCode);
        AddDomainEvent(new PriceSetDomainEvent(Id, _currentPrice));
    }
}