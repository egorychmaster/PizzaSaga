using MassTransit;
using Mediator;
using Order.Application.Features.Orders.UpdateOrderStatus;
using PizzaSaga.Contracts.Orders.IntegrationEvents;

namespace Order.Infrastructure.MassTransit.Consumers;

/// <summary>
/// Consumer для терминального события отмены заказа.
/// Получает событие от Order Saga и обновляет публичный статус агрегата Order на Cancelled.
/// </summary>
public sealed class OrderCancelledConsumer : IConsumer<OrderCancelledIntegrationEvent>
{
    private readonly IMediator _mediator;

    public OrderCancelledConsumer(IMediator mediator)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    /// <inheritdoc />
    public async Task Consume(ConsumeContext<OrderCancelledIntegrationEvent> context)
    {
        await _mediator.Send(
            new UpdateOrderStatusCommand(
                context.Message.OrderId,
                OrderStatusUpdate.Cancelled),
            context.CancellationToken);
    }
}
