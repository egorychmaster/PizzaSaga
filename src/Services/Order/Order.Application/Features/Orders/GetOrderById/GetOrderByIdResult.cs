namespace Order.Application.Features.Orders.GetOrderById;

/// <summary>
/// Результат детализации заказа.
/// </summary>
/// <param name="OrderId">Идентификатор заказа.</param>
/// <param name="CustomerId">Идентификатор клиента.</param>
/// <param name="Status">Текущий статус заказа.</param>
/// <param name="Items">Позиции заказа.</param>
/// <param name="TotalAmount">Общая стоимость заказа.</param>
/// <param name="Currency">Валюта заказа.</param>
/// <param name="CreatedAt">Дата и время создания заказа.</param>
/// <param name="UpdatedAt">Дата и время последнего обновления заказа.</param>
public sealed record GetOrderByIdResult(
    Guid OrderId,
    Guid CustomerId,
    string Status,
    IReadOnlyCollection<GetOrderItemResult> Items,
    decimal TotalAmount,
    string Currency,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

/// <summary>
/// Позиция заказа.
/// </summary>
/// <param name="ProductId">Идентификатор продукта.</param>
/// <param name="Quantity">Количество продукта.</param>
/// <param name="UnitPrice">Цена одной единицы продукта.</param>
public sealed record GetOrderItemResult(
    Guid ProductId,
    int Quantity,
    decimal UnitPrice);
