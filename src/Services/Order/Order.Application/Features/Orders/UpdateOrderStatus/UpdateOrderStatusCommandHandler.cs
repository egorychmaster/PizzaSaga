using Mediator;
using Order.Application.Abstractions.Persistence;

namespace Order.Application.Features.Orders.UpdateOrderStatus;

/// <summary>
/// Обработчик изменения публичного статуса заказа.
/// Вызывается Saga Consumer-ом при переходе в терминальное состояние (Completed / Cancelled).
/// </summary>
public sealed class UpdateOrderStatusCommandHandler(
    IOrderRepository orderRepository) : ICommandHandler<UpdateOrderStatusCommand>
{
    /// <inheritdoc />
    public async ValueTask<Unit> Handle(
        UpdateOrderStatusCommand command,
        CancellationToken cancellationToken)
    {
        var order = await orderRepository.GetByIdForUpdateAsync(command.OrderId, cancellationToken);
        if (order is null)
            throw new InvalidOperationException($"Order not found: OrderId={command.OrderId}");

        switch (command.Status)
        {
            case OrderStatusUpdate.Completed:
                order.MarkCompleted();
                break;

            case OrderStatusUpdate.Cancelled:
                order.MarkCancelled();
                break;

            default:
                throw new ArgumentOutOfRangeException(
                    nameof(command.Status),
                    command.Status,
                    "Unsupported order status update.");
        }

        return Unit.Value;
    }
}
