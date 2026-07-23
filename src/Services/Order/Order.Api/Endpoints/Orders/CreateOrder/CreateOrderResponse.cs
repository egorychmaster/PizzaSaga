namespace Order.Api.Endpoints.Orders.CreateOrder;

public sealed record CreateOrderResponse(
    Guid OrderId,
    string Status,
    DateTimeOffset CreatedAt);