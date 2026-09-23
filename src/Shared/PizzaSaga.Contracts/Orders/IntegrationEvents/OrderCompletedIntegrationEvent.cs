namespace PizzaSaga.Contracts.Orders.IntegrationEvents;

/// <summary>
/// Терминальное событие успешного завершения заказа.
/// Публикуется Order Saga после успешной авторизации платежа — заказ завершён.
/// </summary>
/// <param name="OrderId">Идентификатор завершённого заказа.</param>
/// <param name="CustomerId">Идентификатор клиента, разместившего заказ.</param>
/// <param name="TotalAmount">Общая сумма заказа.</param>
/// <param name="CurrencyCode">Код валюты.</param>
public sealed record OrderCompletedIntegrationEvent(
    Guid OrderId,
    Guid CustomerId,
    decimal TotalAmount,
    string CurrencyCode);
