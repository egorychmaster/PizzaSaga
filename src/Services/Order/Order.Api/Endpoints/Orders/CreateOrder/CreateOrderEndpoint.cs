using Mediator;
using Order.Application.Features.Orders.CreateOrder;
using System.Security.Claims;

namespace Order.Api.Endpoints.Orders.CreateOrder;

public static class CreateOrderEndpoint
{
    public static IEndpointRouteBuilder MapCreateOrderEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost(
                "/api/v1/orders",
                HandleAsync)
            .WithName("CreateOrder")
            .WithTags("Orders")
            .RequireAuthorization()
            .Produces<CreateOrderResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status409Conflict);

        return endpoints;
    }

    private static async Task<IResult> HandleAsync(
        CreateOrderRequest request,
        HttpContext httpContext,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var customerIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier) ?? httpContext.User.FindFirst("sub");
        if (customerIdClaim is null || !Guid.TryParse(customerIdClaim.Value, out var customerId))
            return Results.Unauthorized();

        var command = new CreateOrderCommand(
            CustomerId: customerId,
            Items: request.Items.Select(item => new CreateOrderItem(
                    ProductId: item.ProductId,
                    Quantity: item.Quantity)).ToArray(),
            PaymentMethod: request.PaymentMethod,
            Currency: request.Currency);

        var result = await mediator.Send(command, cancellationToken);

        var response = new CreateOrderResponse(
            OrderId: result.OrderId,
            Status: result.Status,
            CreatedAt: result.CreatedAt);

        return Results.Created(
            $"/api/v1/orders/{result.OrderId}",
            response);
    }
}
