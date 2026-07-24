namespace Order.Application.Features.Orders.GetOrders;

public sealed record GetOrdersResult(
    IReadOnlyCollection<GetOrdersItemResult> Items,
    int Page,
    int PageSize,
    long TotalCount);

public sealed record GetOrdersItemResult(
    Guid OrderId,
    string Status,
    DateTimeOffset CreatedAt);