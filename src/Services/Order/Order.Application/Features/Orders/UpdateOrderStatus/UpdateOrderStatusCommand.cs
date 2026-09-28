using Mediator;

namespace Order.Application.Features.Orders.UpdateOrderStatus;

/// <summary>
/// Команда изменения публичного статуса заказа.
/// Используется внутренним consumer-ом Order Service после завершения Saga.
/// </summary>
/// <param name="OrderId">Идентификатор заказа.</param>
/// <param name="Status">Новый публичный статус.</param>
public sealed record UpdateOrderStatusCommand(
    Guid OrderId,
    OrderStatusUpdate Status) : ICommand;
