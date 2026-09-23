namespace PizzaSaga.Contracts.Stock.IntegrationEvents;

/// <summary>
/// Событие об успешном освобождении ранее созданного резерва товара.
/// Публикуется Stock Service в ответ на ReleaseInventoryIntegrationCommand.
/// Сообщает Order Saga, что компенсация резерва успешно выполнена.
/// </summary>
/// <param name="OrderId">Идентификатор заказа.</param>
public sealed record InventoryReleasedIntegrationEvent(
    Guid OrderId);
