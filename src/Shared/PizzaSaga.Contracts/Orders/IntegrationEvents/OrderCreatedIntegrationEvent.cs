using PizzaSaga.Contracts.Orders.Models;

namespace PizzaSaga.Contracts.Orders.IntegrationEvents;

/// <summary>
/// Интеграционное событие создания заказа, публикуемое в брокер/Outbox.
/// Содержит данные, необходимые Saga для запуска процесса оформления заказа: резервирование товара и авторизацию оплаты.
/// </summary>
/// <param name="OrderId">Идентификатор созданного заказа.</param>
/// <param name="CustomerId">Идентификатор клиента, разместившего заказ.</param>
/// <param name="TotalAmount">Общая сумма заказа в целевых единицах (CurrencyCode).</param>
/// <param name="CurrencyCode">Код валюты (ISO 4217), в которой указана сумма TotalAmount.</param>
/// <param name="CreatedAt">Дата и время создания заказа (UTC/offset-aware).</param>
/// <param name="Items">Коллекция позиций заказа — список пицц с количеством.</param>
public sealed record OrderCreatedIntegrationEvent(
    Guid OrderId,
    Guid CustomerId,
    decimal TotalAmount,
    string CurrencyCode,
    DateTimeOffset CreatedAt,
    IReadOnlyCollection<PizzaItem> Items);
