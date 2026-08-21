using Mediator;
using Order.Application.Abstractions.Persistence;
using Order.Application.Abstractions.Persistence.Idempotency;
using Order.Domain.AggregatesModel.OrderAggregate;
using Order.Domain.ValueObjects;
using System.Text.Json;
using OrderAggregate = Order.Domain.AggregatesModel.OrderAggregate.Order;

namespace Order.Application.Features.Orders.CreateOrder;

/// <summary>
/// Обработчик команды создания заказа.
/// </summary>
public sealed class CreateOrderCommandHandler : ICommandHandler<CreateOrderCommand, CreateOrderResult>
{
    private const decimal TemporaryUnitPrice = 10m;

    private readonly IOrderRepository _orderRepository;
    private readonly IIdempotencyContext _idempotencyContext;
    private readonly IIdempotencyRepository _idempotencyRepository;

    public CreateOrderCommandHandler(IOrderRepository orderRepository
        ,
        IIdempotencyContext idempotencyContext,
        IIdempotencyRepository idempotencyRepository
        )
    {
        _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
        _idempotencyContext = idempotencyContext ?? throw new ArgumentNullException(nameof(idempotencyContext));
        _idempotencyRepository = idempotencyRepository ?? throw new ArgumentNullException(nameof(idempotencyRepository));
    }

    /// <inheritdoc />
    public async ValueTask<CreateOrderResult> Handle(CreateOrderCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        // 1. Создаём Value Objects
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

        // 2. Создаём агрегат Order
        var order = OrderAggregate.Create(
            id: Guid.CreateVersion7(),
            customerId: customerIdentity,
            items: items);

        // 3. Сохраняем через репозиторий (в рамках транзакции TransactionBehavior)
        await _orderRepository.AddAsync(order, cancellationToken);

        // 4. Возвращаем DTO — берём данные из агрегата
        var result = new CreateOrderResult(
            OrderId: order.Id,
            Status: order.Status.ToString(),
            TotalAmount: order.TotalAmount.Amount,
            Currency: order.TotalAmount.CurrencyCode,
            CreatedAt: order.CreatedAt);

        // Запись IdempotencyRecord для идемпотентности.
        if (_idempotencyContext.IsSet)
        {
            var jsonBody = JsonSerializer.Serialize(result);

            var idempotencyRecord = new IdempotencyRecordDto(
                IdempotencyKey: _idempotencyContext.Key,
                RequestHash: _idempotencyContext.RequestHash!,
                ResponseStatusCode: 201,
                ResponseBody: jsonBody,
                CreatedAt: DateTimeOffset.UtcNow);

            await _idempotencyRepository.AddAsync(idempotencyRecord, cancellationToken);
        }

        return result;
    }
}