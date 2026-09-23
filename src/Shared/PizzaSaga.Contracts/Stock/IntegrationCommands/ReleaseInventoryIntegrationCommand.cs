namespace PizzaSaga.Contracts.Stock.IntegrationCommands;

/// <summary>
/// Компенсирующая команда освобождения резерва товара в Stock Service.
/// Публикуется Order Saga при отмене заказа (компенсация после успешного резервирования).
/// </summary>
/// <param name="OrderId">Идентификатор заказа, для которого нужно освободить резерв.</param>
public sealed record ReleaseInventoryIntegrationCommand(
    Guid OrderId);
