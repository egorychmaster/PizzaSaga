using Mediator;
using Order.Application.Features.Orders.CreateOrder;
using Order.Domain.ValueObjects;
using System.Security.Claims;

namespace Order.Api.Endpoints.Orders.CreateOrder;

/// <summary> 
/// Регистрирует HTTP endpoint для создания заказа. 
/// </summary>
public static class CreateOrderEndpoint
{
    /// <summary> 
    /// Регистрирует POST /api/v1/orders. 
    /// </summary> 
    /// <param name="endpoints">Маршруты приложения.</param> 
    /// <returns>Тот же набор маршрутов для дальнейшей конфигурации.</returns>
    public static IEndpointRouteBuilder MapCreateOrderEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/v1/orders", HandleAsync)
            .WithName("CreateOrder")
            .WithTags("Orders")
            .RequireAuthorization()
            .Produces<CreateOrderResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status409Conflict);

        return endpoints;
    }

    /// <summary> 
    /// Обрабатывает запрос создания заказа. 
    /// </summary> 
    private static async Task<IResult> HandleAsync(CreateOrderRequest request, HttpContext httpContext, IMediator mediator, CancellationToken cancellationToken)
    {
        // CustomerId намеренно не принимается из JSON. Идентичность пользователя берётся только из проверенного JWT.
        var customerIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier) ?? httpContext.User.FindFirst("sub");
        if (customerIdClaim is null || !Guid.TryParse(customerIdClaim.Value, out var customerId))
            return Results.Unauthorized();

        // Примитивный Guid преобразуется в доменный Value Object. CustomerIdentity защищает доменный инвариант Guid.Empty.
        var customerIdentity = CustomerIdentity.Create(customerId);

        // Преобразуем транспортные DTO в application/domain-типы. ProductId и Quantity больше не попадут в Command как сырые значения.
        var items = request.Items.Select(item => new CreateOrderItem(
                ProductId: item.ProductId,
                Quantity: PizzaQuantity.Create(item.Quantity))
            ).ToArray();

        var command = new CreateOrderCommand(
            CustomerId: customerIdentity,
            Items: items,
            PaymentMethod: request.PaymentMethod,
            Currency: request.Currency);

        // Передаём команду в Mediator. Дальше управление перейдёт в pipeline behaviors, а затем в CreateOrderCommandHandler.
        var result = await mediator.Send(command, cancellationToken);

        // Application Result преобразуется в HTTP Response DTO.
        var response = new CreateOrderResponse(
            OrderId: result.OrderId,
            Status: result.Status,
            TotalAmount: result.TotalAmount,
            Currency: result.Currency,
            CreatedAt: result.CreatedAt);

        return Results.Created($"/api/v1/orders/{result.OrderId}", response);
    }
}
