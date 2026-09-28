using Mediator;
using Order.Application.Abstractions.Persistence;
using Order.Domain.Abstractions.Repositories;
using Order.Domain.AggregatesModel.Orders;
using Order.Domain.AggregatesModel.Orders.Exceptions;
using Order.Domain.AggregatesModel.Orders.ValueObjects;
using Order.Domain.AggregatesModel.ProductCatalog;
using PizzaSaga.Contracts.Orders.IntegrationEvents;
using PizzaSaga.Contracts.Orders.Models;
using PizzaSaga.SharedKernel.Domain.ValueObjects;

namespace Order.Application.Features.Orders.CreateOrder;

/// <summary>
/// Обработчик команды создания заказа.
/// Создаёт агрегат Order и сохраняет OrderCreatedIntegrationEvent через Outbox.
/// </summary>
public sealed class CreateOrderCommandHandler(
    IOrderRepository orderRepository,
    IProductCatalogRepository productCatalogRepository,
    ICurrencyExchangeRateRepository currencyRateRepo,
    IUnitOfWork unitOfWork) 
    : ICommandHandler<CreateOrderCommand, CreateOrderResult>
{
    private readonly IOrderRepository _orderRepository = orderRepository;
    private readonly IProductCatalogRepository _productCatalogRepository = productCatalogRepository;
    private readonly ICurrencyExchangeRateRepository _currencyRateRepo = currencyRateRepo;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

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
            var product = await _productCatalogRepository.GetProductAsync(item.ProductId, cancellationToken);
            if (product is null)
                throw new ProductNotFoundException(item.ProductId);

            // Конвертируем цену в валюту пользователя
            var userCurrency = Currency.Create(command.Currency);

            var unitPrice = await ConvertToUserCurrencyAsync(product, userCurrency, cancellationToken);

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

        // 5. Формируем интеграционное событие и сохраняем его в Outbox.
        // Это выполнится внутри той же PostgreSQL-транзакции перед SaveChanges и Commit.
        var integrationEvent = new OrderCreatedIntegrationEvent(
            OrderId: order.Id,
            CustomerId: order.CustomerId.Value,
            TotalAmount: order.TotalAmount.Amount,
            CurrencyCode: order.TotalAmount.Currency.Code,
            CreatedAt: order.CreatedAt,
            Items: order.Items.Select(i => new PizzaItem(
                i.ProductId,
                i.Quantity.Value)).ToArray());

        await _unitOfWork.SaveWithOutboxAsync(order.Id, integrationEvent, cancellationToken);

        // 6. Возвращаем DTO — берём данные из агрегата
        var result = new CreateOrderResult(
            OrderId: order.Id,
            Status: order.Status.ToString(),
            TotalAmount: order.TotalAmount.Amount,
            Currency: order.TotalAmount.Currency.Code,
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
        Currency userCurrency,
        CancellationToken cancellationToken)
    {
        // Проверка: цена всегда в USD
        if (product.CurrencyCode != "USD")
            throw new InvalidOperationException($"Product {product.ProductId} price must be in USD, but is {product.CurrencyCode}.");

        // Если пользователь заказывает в USD — ничего не конвертируем
        if (userCurrency == Currency.USD)
            return Money.Create(product.PriceAmount, Currency.USD);

        // Иначе получаем курс USD → userCurrency и конвертируем
        var rate = await _currencyRateRepo.GetRateAsync("USD", userCurrency.Code, cancellationToken);

        var convertedAmount = product.PriceAmount * rate;

        return Money.Create(convertedAmount, userCurrency);
    }
}
