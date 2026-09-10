namespace Order.Domain.AggregatesModel.ProductCatalog;

/// <summary>
/// Локальный кэш каталога продуктов для Order Service.
/// Используется для валидации и получения фиксированной цены при создании заказа.
/// </summary>
public sealed class ProductCatalogCache
{
    /// <summary>
    /// Идентификатор продукта (PK).
    /// </summary>
    public Guid ProductId { get; set; }

    /// <summary>
    /// Наименование продукта.
    /// </summary>
    public string Name { get; set; } = default!;

    /// <summary>
    /// Описание продукта (опционально).
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Цена продукта (сумма).
    /// </summary>
    public decimal PriceAmount { get; set; }

    /// <summary>
    /// Код валюты (в формате ISO 4217).
    /// </summary>
    public string CurrencyCode { get; set; } = default!;
}
