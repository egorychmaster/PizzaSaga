namespace PizzaSaga.Contracts.IntegrationEvents.Catalogs;

/// <summary>
/// Интеграционное событие создания продукта.
/// Публикуется при добавлении нового продукта в Catalog Service.
/// </summary>
public sealed record ProductCreatedIntegrationEvent(
    Guid ProductId,
    string Name,
    decimal PriceAmount,
    string CurrencyCode);