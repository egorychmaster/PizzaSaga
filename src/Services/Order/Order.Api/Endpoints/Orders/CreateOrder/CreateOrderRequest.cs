namespace Order.Api.Endpoints.Orders.CreateOrder;

/// <summary> 
/// Входной HTTP-запрос для создания нового заказа. 
/// </summary> 
/// <param name="Items">Позиции создаваемого заказа.</param> 
/// <param name="PaymentMethod">Способ оплаты заказа.</param> 
/// <param name="Currency">Валюта заказа.</param>
public sealed record CreateOrderRequest(
    IReadOnlyCollection<CreateOrderItemRequest> Items,
    string PaymentMethod,    
    string Currency);

/// <summary> 
/// Позиция заказа, передаваемая клиентом. 
/// </summary> 
/// <param name="ProductId">Идентификатор продукта.</param> 
/// <param name="Quantity">Количество единиц продукта.</param>
public sealed record CreateOrderItemRequest(
    Guid ProductId,
    int Quantity);