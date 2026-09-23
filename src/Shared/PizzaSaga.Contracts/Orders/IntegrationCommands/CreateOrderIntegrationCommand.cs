namespace PizzaSaga.Contracts.Orders.IntegrationCommands;

/// <summary>
/// Инициирующая команда создания заказа.
/// Публикуется Order Service при поступлении запроса на создание заказа от клиента.
/// Запускает распределённую транзакцию (Saga) через Order Saga.
/// </summary>
public sealed record CreateOrderIntegrationCommand(
    Guid CustomerId,
    string Currency,
    decimal TotalAmount,
    IEnumerable<ProductItem> Items);

/// <summary>
/// Позиция заказа — продукт и количество для резервирования в Stock Service.
/// </summary>
/// <param name="ProductId">Идентификатор продукта из Catalog.</param>
/// <param name="Quantity">Количество единиц товара.</param>
public sealed record ProductItem(
    Guid ProductId,
    int Quantity);
