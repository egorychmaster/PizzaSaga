namespace Order.Api.Endpoints.Orders.CreateOrder;

public sealed record CreateOrderRequest(
    IReadOnlyCollection<CreateOrderItemRequest> Items,
    string PaymentMethod,
    string Currency);

public sealed record CreateOrderItemRequest(
    Guid ProductId,
    int Quantity);