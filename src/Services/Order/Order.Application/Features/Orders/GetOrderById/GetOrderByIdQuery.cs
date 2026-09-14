using Mediator;

namespace Order.Application.Features.Orders.GetOrderById;

/// <summary>
/// Запрос на получение детализации заказа по идентификатору.
/// </summary>
/// <param name="OrderId">Идентификатор заказа.</param>
public sealed record GetOrderByIdQuery(Guid OrderId)
    : IQuery<GetOrderByIdResult>;
