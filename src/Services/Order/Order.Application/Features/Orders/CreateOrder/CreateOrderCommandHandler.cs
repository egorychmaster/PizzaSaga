using Mediator;

namespace Order.Application.Features.Orders.CreateOrder;

public sealed class CreateOrderCommandHandler : ICommandHandler<CreateOrderCommand, CreateOrderResult>
{
    public ValueTask<CreateOrderResult> Handle(
        CreateOrderCommand command,
        CancellationToken cancellationToken)
    {
        var orderId = Guid.CreateVersion7();

        var response = new CreateOrderResult(
            OrderId: orderId,
            Status: "Pending",
            CreatedAt: DateTimeOffset.UtcNow);

        return ValueTask.FromResult(response);
    }
}