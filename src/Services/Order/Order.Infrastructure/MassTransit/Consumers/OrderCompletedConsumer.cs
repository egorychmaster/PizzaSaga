using MassTransit;
using Mediator;
using Order.Application.Features.Orders.UpdateOrderStatus;
using PizzaSaga.Contracts.Orders.IntegrationEvents;

namespace Order.Infrastructure.MassTransit.Consumers;

/// <summary>
/// Consumer для терминального события успешного завершения заказа.
/// Получает событие от Order Saga и обновляет публичный статус агрегата Order на Completed.
/// </summary>
public sealed class OrderCompletedConsumer : IConsumer<OrderCompletedIntegrationEvent>
{
    private readonly IMediator _mediator;

    public OrderCompletedConsumer(IMediator mediator)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    /// <inheritdoc />
    public async Task Consume(ConsumeContext<OrderCompletedIntegrationEvent> context)
    {
        await _mediator.Send(
            new UpdateOrderStatusCommand(
                context.Message.OrderId,
                OrderStatusUpdate.Completed),
            context.CancellationToken);
    }
}
