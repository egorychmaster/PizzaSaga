namespace PizzaSaga.Contracts.Orders.Models;

/// <summary>
/// Позиция пиццы в заказе.
/// Используется в интеграционных командах и событиях для передачи позиций заказа между сервисами.
/// </summary>
/// <param name="ProductId">Идентификатор продукта из Catalog Service.</param>
/// <param name="Quantity">Количество единиц товара.</param>
public sealed record PizzaItem(
    Guid ProductId,
    int Quantity);
