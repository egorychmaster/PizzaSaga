using Mediator;
using Order.Domain.ValueObjects;

namespace Order.Application.Features.Orders.CreateOrder;

public sealed record CreateOrderCommand(
    CustomerIdentity CustomerId,
    IReadOnlyCollection<CreateOrderItem> Items,
    string PaymentMethod,
    string Currency)
    : ICommand<CreateOrderResult>;

public sealed record CreateOrderItem(
    Guid ProductId,
    PizzaQuantity Quantity);