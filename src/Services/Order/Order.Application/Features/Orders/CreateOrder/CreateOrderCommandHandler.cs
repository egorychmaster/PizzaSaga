using Mediator;
using Order.Application.Abstractions.Persistence;
using Order.Domain.Abstractions.Repositories;
using Order.Domain.AggregatesModel.Orders;
using Order.Domain.AggregatesModel.Orders.Exceptions;
using Order.Domain.AggregatesModel.Orders.ValueObjects;

namespace Order.Application.Features.Orders.CreateOrder;

/// <summary>
/// Обработчик команды создания заказа.
/// </summary>
public sealed class CreateOrderCommandHandler : ICommandHandler<CreateOrderCommand, CreateOrderResult>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductCatalogRepository _productCatalogRepository;

    public CreateOrderCommandHandler(
        IOrderRepository orderRepository,
        IProductCatalogRepository productCatalogRepository)
    {
        _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
        _productCatalogRepository = productCatalogRepository ?? throw new ArgumentNullException(nameof(productCatalogRepository));
    }

    /// <inheritdoc />
    public async ValueTask<CreateOrderResult> Handle(CreateOrderCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        // 1. Создаём Value Objects
        var customerIdentity = CustomerIdentity.Create(command.CustomerId.Value);

        // 2. Проверяем наличие продуктов в кэше и готовим OrderItem'ы
        var items = new List<OrderItem>();
        foreach (var item in command.Items)
        {
            // Получаем данные из кэша
            var product = _productCatalogRepository.GetProductAsync(item.ProductId, cancellationToken).GetAwaiter().GetResult();
            if (product is null)
                throw new ProductNotFoundException(item.ProductId);

            // Создаём Money из кэшированных данных
            var unitPrice = Money.Create(product.PriceAmount, product.CurrencyCode);

            var quantity = PizzaQuantity.Create(item.Quantity.Value);

            items.Add(new OrderItem(
                id: Guid.CreateVersion7(),
                productId: item.ProductId,
                quantity: quantity,
                unitPrice: unitPrice));
        }

        // 3. Создаём агрегат Order
        var order = OrderAggregate.Create(
            id: Guid.CreateVersion7(),
            customerId: customerIdentity,
            items: items);

        // 4. Сохраняем через репозиторий (в рамках транзакции TransactionBehavior)
        await _orderRepository.AddAsync(order, cancellationToken);

        // 5. Возвращаем DTO — берём данные из агрегата
        var result = new CreateOrderResult(
            OrderId: order.Id,
            Status: order.Status.ToString(),
            TotalAmount: order.TotalAmount.Amount,
            Currency: order.TotalAmount.CurrencyCode,
            CreatedAt: order.CreatedAt);

        return result;
    }
}