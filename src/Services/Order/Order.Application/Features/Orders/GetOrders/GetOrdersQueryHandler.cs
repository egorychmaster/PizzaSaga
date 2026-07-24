using Mediator;

namespace Order.Application.Features.Orders.GetOrders;

public class GetOrdersQueryHandler : IQueryHandler<GetOrdersQuery, GetOrdersResult>
{
    public ValueTask<GetOrdersResult> Handle(GetOrdersQuery request, CancellationToken cancellationToken)
    {
        List<GetOrdersItemResult> items =
            new List<GetOrdersItemResult>
            {
                new GetOrdersItemResult(OrderId: Guid.NewGuid(), Status: "Pending", CreatedAt: DateTimeOffset.UtcNow),
                new GetOrdersItemResult(OrderId: Guid.NewGuid(), Status: "Baking", CreatedAt: DateTimeOffset.UtcNow)
            };

        var results = new GetOrdersResult(
            Items: items,
            Page: request.Page,
            PageSize: request.PageSize,
            TotalCount: items.Count
        );

        return new ValueTask<GetOrdersResult>(results);
    }
}