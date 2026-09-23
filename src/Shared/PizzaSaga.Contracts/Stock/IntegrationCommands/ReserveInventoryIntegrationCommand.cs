namespace PizzaSaga.Contracts.Stock.IntegrationCommands;

/// <summary>
/// Команда на резервирование товара в Stock Service.
/// Публикуется Order Saga при создании заказа.
/// Содержит идентификатор заказа и перечень продуктов с требуемым количеством.
/// </summary>
public sealed record ReserveInventoryIntegrationCommand(
    Guid OrderId,
    IEnumerable<ProductQuantity> Products);

/// <summary>
/// Информация о продукте и количестве для резервирования.
/// </summary>
/// <param name="ProductId">Идентификатор продукта.</param>
/// <param name="Quantity">Количество для резервирования.</param>
public sealed record ProductQuantity(
    Guid ProductId,
    int Quantity);
