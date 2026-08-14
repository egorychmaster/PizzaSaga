using Mediator;
using Order.Application.Abstractions.Persistence;
using Order.Domain.AggregatesModel.OrderAggregate;
using Order.Domain.ValueObjects;
using OrderAggregate = Order.Domain.AggregatesModel.OrderAggregate.Order;

namespace Order.Application.Features.Orders.CreateOrder;

/// <summary>
/// Обработчик команды создания заказа.
/// </summary>
public sealed class CreateOrderCommandHandler : ICommandHandler<CreateOrderCommand, CreateOrderResult>
{
    private const decimal TemporaryUnitPrice = 10m;

    private readonly IOrderRepository _orderRepository;

    public CreateOrderCommandHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository
            ?? throw new ArgumentNullException(nameof(orderRepository));
    }

    /// <inheritdoc />
    public async ValueTask<CreateOrderResult> Handle(CreateOrderCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var customerIdentity = CustomerIdentity.Create(command.CustomerId.Value);

        var items = command.Items
            .Select(item =>
            {
                var quantity = PizzaQuantity.Create(item.Quantity.Value);

                var unitPrice = Money.Create(TemporaryUnitPrice, command.Currency);

                return new OrderItem(
                    id: Guid.CreateVersion7(),
                    productId: item.ProductId,
                    quantity: quantity,
                    unitPrice: unitPrice);
            })
            .ToArray();

        var order = OrderAggregate.Create(
            id: Guid.CreateVersion7(),
            customerId: customerIdentity,
            items: items);

        await _orderRepository.AddAsync(
            order,
            cancellationToken);

        CreateOrderResult response = new CreateOrderResult(
            OrderId: order.Id,
            Status: order.Status.ToString(),
            TotalAmount: order.TotalAmount.Amount,
            Currency: order.TotalAmount.CurrencyCode,
            CreatedAt: order.CreatedAt);

        //var orderId = Guid.CreateVersion7();
        //var response = new CreateOrderResult(
        //     OrderId: orderId,
        //     Status: "Pending",
        //     TotalAmount: 0m,
        //     Currency: command.Currency,
        //     CreatedAt: DateTimeOffset.UtcNow);

        //return ValueTask.FromResult(response);
        return response;
    }
}