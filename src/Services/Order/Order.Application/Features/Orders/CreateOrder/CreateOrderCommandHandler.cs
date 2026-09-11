using Mediator;
using Order.Application.Abstractions.Persistence;
using Order.Domain.Abstractions.Repositories;
using Order.Domain.AggregatesModel.Orders;
using Order.Domain.AggregatesModel.Orders.Exceptions;
using Order.Domain.AggregatesModel.Orders.ValueObjects;
using Order.Domain.AggregatesModel.ProductCatalog;

namespace Order.Application.Features.Orders.CreateOrder;

/// <summary>
/// Обработчик команды создания заказа.
/// </summary>
public sealed class CreateOrderCommandHandler : ICommandHandler<CreateOrderCommand, CreateOrderResult>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductCatalogRepository _productCatalogRepository;
    private readonly ICurrencyExchangeRateRepository _currencyRateRepo;

    public CreateOrderCommandHandler(
        IOrderRepository orderRepository,
        IProductCatalogRepository productCatalogRepository,
        ICurrencyExchangeRateRepository currencyRateRepo)
    {
        _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
        _productCatalogRepository = productCatalogRepository ?? throw new ArgumentNullException(nameof(productCatalogRepository));
        _currencyRateRepo = currencyRateRepo ?? throw new ArgumentNullException(nameof(currencyRateRepo));
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

            // Конвертируем цену в валюту пользователя
            var unitPrice = await ConvertToUserCurrencyAsync(product, command.Currency, cancellationToken);

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

    /// <summary>
    /// Конвертирует цену продукта в валюту пользователя.
    /// Если пользователь заказывает в USD — цена не меняется.
    /// Иначе берёт курс из CurrencyExchangeRates и умножает.
    /// </summary>
    private async Task<Money> ConvertToUserCurrencyAsync(
        ProductCatalogCache product,
        string userCurrency,
        CancellationToken cancellationToken)
    {
        // Проверка: цена всегда в USD
        if (product.CurrencyCode != "USD")
            throw new InvalidOperationException($"Product {product.ProductId} price must be in USD, but is {product.CurrencyCode}.");

        // Если пользователь заказывает в USD — ничего не конвертируем
        if (userCurrency.Equals("USD", StringComparison.OrdinalIgnoreCase))
            return Money.Create(product.PriceAmount, "USD");

        // Иначе получаем курс USD → userCurrency и конвертируем
        var rate = await _currencyRateRepo.GetRateAsync("USD", userCurrency, cancellationToken);
        var convertedAmount = product.PriceAmount * rate;

        return Money.Create(convertedAmount, userCurrency);
    }
}