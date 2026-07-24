using Mediator;

namespace Order.Application.Features.Orders.GetOrders;

public sealed record GetOrdersQuery(
    Guid CustomerId,
    int Page = 1,
    int PageSize = 10)
    : IQuery<GetOrdersResult>;

