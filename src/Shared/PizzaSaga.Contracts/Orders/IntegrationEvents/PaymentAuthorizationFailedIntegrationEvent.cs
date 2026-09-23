namespace PizzaSaga.Contracts.Orders.IntegrationEvents;

/// <summary>
/// Событие неудачной авторизации платежа в Payment Service.
/// Публикуется при отказе платёжного шлюза (недостаточно средств, карта отклонена и т.д.).
/// Order Saga обрабатывает это событие и запускает компенсацию (откат резерва товара).
/// </summary>
/// <param name="OrderId">Идентификатор заказа, для которого не удалось авторизовать платёж.</param>
/// <param name="Amount">Сумма платежа.</param>
/// <param name="CurrencyCode">Код валюты.</param>
/// <param name="Reason">Причина отказа (например, "Недостаточно средств на карте").</param>
public sealed record PaymentAuthorizationFailedIntegrationEvent(
    Guid OrderId,
    decimal Amount,
    string CurrencyCode,
    string Reason);
