namespace PizzaSaga.Contracts.Stock.IntegrationEvents;

/// <summary>
/// Событие об успешном резервировании товара.
/// Публикуется Stock Service в ответ на ReserveInventoryIntegrationCommand.
/// Сообщает Order Saga, что необходимое количество продуктов успешно зарезервировано.
/// </summary>
/// <param name="OrderId">Идентификатор заказа.</param>
public sealed record InventoryReservedIntegrationEvent(
    Guid OrderId);
