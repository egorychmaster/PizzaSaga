using Mediator;

namespace Order.Application.Features.Orders.CreateOrder;

public sealed record CreateOrderCommand(
    Guid CustomerId,
    IReadOnlyCollection<CreateOrderItem> Items,
    string PaymentMethod,
    string Currency)
    : ICommand<CreateOrderResult>;

public sealed record CreateOrderItem(
    Guid ProductId,
    int Quantity);