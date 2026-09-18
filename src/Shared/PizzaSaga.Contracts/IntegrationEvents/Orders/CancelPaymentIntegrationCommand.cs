namespace PizzaSaga.Contracts.IntegrationEvents.Orders;

/// <summary>
/// Команда для отмены платежа по заказу.
/// Публикуется Order.Service при отмене заказа.
/// </summary>
/// <param name="OrderId">Идентификатор заказа.</param>
public record CancelPaymentIntegrationCommand(
    Guid OrderId);
