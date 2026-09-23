namespace PizzaSaga.Contracts.Orders.IntegrationEvents;

/// <summary>
/// Событие об успешной авторизации платежа.
/// Публикуется Payment Service в ответ на AuthorizePaymentIntegrationCommand.
/// </summary>
/// <param name="OrderId">Идентификатор заказа.</param>
public sealed record PaymentAuthorizedIntegrationEvent(
    Guid OrderId);
