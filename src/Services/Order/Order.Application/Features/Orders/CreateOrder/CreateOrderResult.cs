namespace Order.Application.Features.Orders.CreateOrder;

public sealed record CreateOrderResult(
    Guid OrderId,
    string Status,
    DateTimeOffset CreatedAt);