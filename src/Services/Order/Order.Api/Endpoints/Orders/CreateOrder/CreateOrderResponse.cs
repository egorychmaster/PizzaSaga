namespace Order.Api.Endpoints.Orders.CreateOrder;

/// <summary> 
/// Ответ API на успешное создание заказа. 
/// </summary> 
/// <param name="OrderId">Уникальный идентификатор созданного заказа.</param> 
/// <param name="Status">Текущий статус заказа.</param> 
/// <param name="TotalAmount">Общая стоимость заказа, рассчитанная сервером.</param> 
/// <param name="Currency">Валюта, в которой указана общая стоимость заказа.</param> 
/// <param name="CreatedAt">Дата и время создания заказа в формате UTC.</param>
public sealed record CreateOrderResponse(
    Guid OrderId,
    string Status,
    decimal TotalAmount,
    string Currency,
    DateTimeOffset CreatedAt);