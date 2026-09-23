namespace PizzaSaga.Contracts.Payment.IntegrationCommands;

/// <summary>
/// Команда для авторизации платежа по заказу.
/// Обрабатывается Payment Service.
/// </summary>
/// <param name="OrderId">Идентификатор заказа.</param>
/// <param name="Amount">Сумма к списанию.</param>
/// <param name="CurrencyCode">Код валюты (например, "RUB").</param>
public record AuthorizePaymentIntegrationCommand(
    Guid OrderId,
    decimal Amount,
    string CurrencyCode);
