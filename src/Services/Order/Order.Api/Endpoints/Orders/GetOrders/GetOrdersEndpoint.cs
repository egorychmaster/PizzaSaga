using Mediator;
using Order.Application.Features.Orders.GetOrders;
using System.Security.Claims;

namespace Order.Api.Endpoints.Orders.GetOrders;

public static class GetOrdersEndpoint
{
    public static IEndpointRouteBuilder MapGetOrdersEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet(
                "/api/v1/orders",
                HandleAsync)
            .WithName("GetOrders")
            .WithTags("Orders")
            .RequireAuthorization()
            .Produces<GetOrdersResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        return endpoints;
    }

    private static async Task<IResult> HandleAsync(
        HttpContext httpContext,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var customerIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier) ?? httpContext.User.FindFirst("sub");
        if (customerIdClaim is null || !Guid.TryParse(customerIdClaim.Value, out var customerId))
            return Results.Unauthorized();

        var query = new GetOrdersQuery(CustomerId: customerId);

        var result = await mediator.Send(query, cancellationToken);

        var response = new GetOrdersResponse(
            Items: result.Items.Select(order => new GetOrdersItemResponse(
                OrderId: order.OrderId,
                Status: order.Status,
                CreatedAt: order.CreatedAt)).ToArray(),
            Page: result.Page,
            PageSize: result.PageSize,
            TotalCount: result.TotalCount
        );

        return Results.Ok(response);
    }
}