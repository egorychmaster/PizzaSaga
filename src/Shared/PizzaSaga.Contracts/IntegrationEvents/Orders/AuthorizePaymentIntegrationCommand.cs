namespace PizzaSaga.Contracts.IntegrationEvents.Orders;

/// <summary>
/// Команда для авторизации платежа по заказу.
/// Публикуется Order.Service при создании заказа.
/// </summary>
/// <param name="OrderId">Идентификатор заказа.</param>
/// <param name="Amount">Сумма к списанию.</param>
/// <param name="CurrencyCode">Код валюты (например, "RUB").</param>
public record AuthorizePaymentIntegrationCommand(
    Guid OrderId,
    decimal Amount,
    string CurrencyCode);
