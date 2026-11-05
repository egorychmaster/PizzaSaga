using Mediator;
using Order.Application.Features.Orders.GetOrderById;

namespace Order.Api.Endpoints.Orders.GetOrderById;

/// <summary>
/// Регистрирует HTTP endpoint для получения детализации заказа.
/// </summary>
public static class GetOrderByIdEndpoint
{
    /// <summary>
    /// Регистрирует GET /api/v1/orders/{Id} внутри versioned route группы версии 1.0.
    /// </summary>
    /// <param name="endpoints">Группа маршрутов версии v1 (/api/v1/orders).</param>
    /// <returns>Та же группа для дальнейшей конфигурации.</returns>
    public static RouteGroupBuilder MapGetOrderByIdEndpoint(this RouteGroupBuilder endpoints)
    {
        // Путь относительно группы: группа уже содержит /api/v{version:apiVersion}/orders.
        endpoints.MapGet("/{Id}", HandleAsync)
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
