using Mediator;
using Order.Application.Features.Orders.GetOrders;
using System.Security.Claims;

namespace Order.Api.Endpoints.Orders.GetOrders;

public static class GetOrdersEndpoint
{
    /// <summary>
    /// Регистрирует GET /api/v1/orders внутри versioned route группы версии 1.0.
    /// </summary>
    /// <param name="endpoints">Группа маршрутов версии v1 (/api/v1/orders).</param>
    /// <returns>Та же группа для дальнейшей конфигурации.</returns>
    public static RouteGroupBuilder MapGetOrdersEndpoint(this RouteGroupBuilder endpoints)
    {
        // Путь относительно группы: группа уже содержит /api/v{version:apiVersion}/orders.
        endpoints.MapGet(
                "",
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