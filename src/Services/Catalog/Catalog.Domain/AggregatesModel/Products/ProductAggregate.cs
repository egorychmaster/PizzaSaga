using Catalog.Domain.AggregatesModel.Products.Events;
using Catalog.Domain.AggregatesModel.Products.Exceptions.ProductAggregates;
using Catalog.Domain.AggregatesModel.Products.ValueObjects;
using PizzaSaga.SharedKernel.Domain;

namespace Catalog.Domain.AggregatesModel.Products;

/// <summary>
/// Агрегат продукта.
/// Управляет информацией о продукте и его текущей ценой.
/// </summary>
public sealed class ProductAggregate : AggregateRootWithId
{
    private Price _currentPrice;

    public string Name { get; private set; } = null!;
    public string Description { get; private set; } = null!;

    /// <summary>
    /// Текущая цена (одна активная запись).
    /// </summary>
    public Price CurrentPrice => _currentPrice;

    // EF Core
    private ProductAggregate() { }

    public ProductAggregate(Guid id, string name, string description, decimal amount, string currencyCode)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new InvalidProductNameException();

        Id = id;
        Name = name.Trim();
        Description = description?.Trim() ?? "";
        _currentPrice = Price.Create(amount, currencyCode);

        AddDomainEvent(new ProductCreatedDomainEvent(Id, Name, Description, amount, currencyCode));
    }

    public static ProductAggregate Create(Guid id, string name, string description, decimal amount, string currencyCode)
        => new(id, name, description, amount, currencyCode);
}