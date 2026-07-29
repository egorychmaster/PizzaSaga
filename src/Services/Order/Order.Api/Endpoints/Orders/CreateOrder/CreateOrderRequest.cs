namespace Order.Api.Endpoints.Orders.CreateOrder;

/// <summary>
/// 
/// </summary>
/// <param name="Items"></param>
/// <param name="PaymentMethod"></param>
/// <param name="Currency">Допустимые коды: EUR/USD/RUB</param>
public sealed record CreateOrderRequest(
    IReadOnlyCollection<CreateOrderItemRequest> Items,
    string PaymentMethod,    
    string Currency);

public sealed record CreateOrderItemRequest(
    Guid ProductId,
    int Quantity);