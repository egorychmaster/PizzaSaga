namespace PizzaSaga.Contracts.Stock.IntegrationEvents;

/// <summary>
/// Событие неудачного резервирования товара в Stock Service.
/// Публикуется при нехватке товара или ошибке резервирования.
/// Order Saga обрабатывает это событие и запускает компенсацию (откат).
/// </summary>
/// <param name="OrderId">Идентификатор заказа, для которого не удалось зарезервировать товар.</param>
/// <param name="Reason">Причина отказа (например, "Недостаточно товара на складе").</param>
/// <param name="ProductId">Идентификатор продукта, по которому возникла ошибка.</param>
/// <param name="RequestedQuantity">Запрошенное количество.</param>
/// <param name="AvailableQuantity">Доступное количество на момент проверки.</param>
public sealed record InventoryReservationFailedIntegrationEvent(
    Guid OrderId,
    string Reason,
    Guid? ProductId = null,
    int? RequestedQuantity = null,
    int? AvailableQuantity = null
    );
