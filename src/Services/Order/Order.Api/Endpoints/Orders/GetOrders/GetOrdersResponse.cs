namespace Order.Api.Endpoints.Orders.GetOrders;

public sealed record GetOrdersResponse(
    IReadOnlyCollection<GetOrdersItemResponse> Items,
    int Page,
    int PageSize,
    long TotalCount);

public sealed record GetOrdersItemResponse(
    Guid OrderId,
    string Status,
    DateTimeOffset CreatedAt);