namespace PizzaSaga.Contracts.Payment.IntegrationEvents;

/// <summary>
/// Событие об успешной отмене платежа (возврате средств).
/// Публикуется Payment Service в ответ на CancelPaymentIntegrationCommand.
/// Сообщает Order Saga, что компенсация платежа успешно выполнена.
/// </summary>
/// <param name="OrderId">Идентификатор заказа.</param>
public sealed record PaymentReleasedIntegrationEvent(
    Guid OrderId);
