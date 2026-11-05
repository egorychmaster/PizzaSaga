using Mediator;
using Order.Api.Idempotency;
using Order.Application.Abstractions.Persistence.Idempotency;
using Order.Application.Features.Orders.CreateOrder;
using Order.Domain.AggregatesModel.Orders.ValueObjects;
using System.Security.Claims;

namespace Order.Api.Endpoints.Orders.CreateOrder;

/// <summary> 
/// Регистрирует HTTP endpoint для создания заказа. 
/// </summary>
public static class CreateOrderEndpoint
{
    /// <summary> 
    /// Регистрирует POST /api/v1/orders внутри versioned route группы версии 1.0.
    /// </summary> 
    /// <param name="endpoints">Группа маршрутов версии v1 (/api/v1/orders).</param> 
    /// <returns>Та же группа для дальнейшей конфигурации.</returns>
    public static RouteGroupBuilder MapCreateOrderEndpoint(this RouteGroupBuilder endpoints)
    {
        // Путь относительно группы: группа уже содержит /api/v{version:apiVersion}/orders.
        endpoints.MapPost("", HandleAsync)
            .RequireAuthorization()
            .Produces<CreateOrderResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .WithName("CreateOrder")
            .WithTags("Orders")
            .AddEndpointFilter<IdempotencyFilter>()
            ;

        return endpoints;
    }

    /// <summary> 
    /// Обрабатывает запрос создания заказа. 
    /// </summary> 
    private static async Task<IResult> HandleAsync(
        CreateOrderRequest request,
        HttpContext httpContext,
        IMediator mediator,
        IIdempotencyContext idempotencyContext,
        CancellationToken cancellationToken)
    {
        // CustomerId намеренно не принимается из JSON. Идентичность пользователя берётся только из проверенного JWT.
        var customerIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier) ??
                              httpContext.User.FindFirst("sub");
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

        // Устанавливаем ожидаемый HTTP-статус ответа
        idempotencyContext.SetResponseStatusCode(StatusCodes.Status201Created);

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
