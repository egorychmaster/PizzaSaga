using Mediator;
using Order.Application.Features.Orders.GetOrderById;

namespace Order.Api.Endpoints.Orders.GetOrderById;

/// <summary>
/// Регистрирует HTTP endpoint для получения детализации заказа.
/// </summary>
public static class GetOrderByIdEndpoint
{
    /// <summary>
    /// Регистрирует GET /api/v1/orders/{Id}.
    /// </summary>
    /// <param name="endpoints">Маршруты приложения.</param>
    /// <returns>Тот же набор маршрутов для дальнейшей конфигурации.</returns>
    public static IEndpointRouteBuilder MapGetOrderByIdEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/v1/orders/{Id}", HandleAsync)
            .WithName("GetOrderById")
            .WithTags("Orders")
            .RequireAuthorization()
            .Produces<GetOrderByIdResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound);

        return endpoints;
    }

    /// <summary>
    /// Обрабатывает запрос получения детализации заказа.
    /// </summary>
    private static async Task<IResult> HandleAsync(
        Guid Id,
        HttpContext httpContext,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var query = new GetOrderByIdQuery(OrderId: Id);

        var result = await mediator.Send(query, cancellationToken);

        var response = new GetOrderByIdResponse(
            OrderId: result.OrderId,
            CustomerId: result.CustomerId,
            Status: result.Status,
            Items: result.Items.Select(item => new GetOrderItemResponse(
                ProductId: item.ProductId,
                Quantity: item.Quantity,
                UnitPrice: item.UnitPrice)).ToArray(),
            TotalAmount: result.TotalAmount,
            Currency: result.Currency,
            CreatedAt: result.CreatedAt,
            UpdatedAt: result.UpdatedAt);

        return Results.Ok(response);
    }
}
