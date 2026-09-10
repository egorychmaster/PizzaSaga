namespace PizzaSaga.Contracts.IntegrationEvents.Catalogs;

/// <summary>
/// Интеграционное событие создания продукта.
/// Публикуется при добавлении нового продукта в Catalog Service.
/// </summary>
/// <param name="ProductId">Уникальный идентификатор созданного продукта.</param>
/// <param name="Name">Наименование продукта.</param>
/// <param name="Description">Описание продукта.</param>
/// <param name="PriceAmount">Цена продукта.</param>
/// <param name="CurrencyCode">Код валюты в формате ISO 4217 (например, "USD", "EUR", "RUB").</param>
public sealed record ProductCreatedIntegrationEvent(
    Guid ProductId,
    string Name,
    string Description,
    decimal PriceAmount,
    string CurrencyCode);