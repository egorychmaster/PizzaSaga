namespace Order.Application.Features.Orders.CreateOrder;

public sealed record CreateOrderResult(
    Guid OrderId,
    string Status,
    decimal TotalAmount,
    string Currency,
    DateTimeOffset CreatedAt);