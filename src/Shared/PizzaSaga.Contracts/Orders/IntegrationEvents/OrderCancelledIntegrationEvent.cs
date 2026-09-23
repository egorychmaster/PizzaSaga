namespace PizzaSaga.Contracts.Orders.IntegrationEvents;

/// <summary>
/// Терминальное событие отмены заказа.
/// Публикуется Order Saga после успешной компенсации (освобождение резерва + отмена платежа).
/// </summary>
/// <param name="OrderId">Идентификатор отменённого заказа.</param>
/// <param name="CustomerId">Идентификатор клиента, разместившего заказ.</param>
public sealed record OrderCancelledIntegrationEvent(
    Guid OrderId,
    Guid CustomerId);
