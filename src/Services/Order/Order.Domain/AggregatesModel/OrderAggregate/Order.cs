using Order.Domain.AggregatesModel.OrderAggregate.Events;
using Order.Domain.Common;
using Order.Domain.ValueObjects;

namespace Order.Domain.AggregatesModel.OrderAggregate;

/// <summary>
/// Агрегат заказа.
/// Управляет состоянием заказа, его позициями и бизнес-инвариантами.
/// </summary>
public sealed class Order : AggregateRoot
{
    private readonly List<OrderItem> _items = [];

    /// <summary>
    /// Идентификатор заказа.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Текущий публичный статус заказа.
    /// </summary>
    public OrderStatus Status { get; private set; }

    /// <summary>
    /// Дата и время создания заказа.
    /// </summary>
    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Идентификатор клиента.
    /// </summary>
    public CustomerIdentity CustomerId { get; private set; } = null!;

    /// <summary>
    /// Зафиксированная общая стоимость заказа.
    /// </summary>
    public Money TotalAmount { get; private set; } = null!;

    /// <summary>
    /// Версия агрегата для оптимистичной блокировки (Optimistic Concurrency).
    /// Инкрементируется при каждом изменении состояния агрегата.
    /// </summary>
    public int Version { get; private set; }

    /// <summary>
    /// Позиции заказа.
    /// </summary>
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();


    // Пустой конструктор для EF Core
    private Order() { }

    /// <summary>
    /// Создаёт новый заказ с указанным идентификатором и клиентом.
    /// </summary>
    public Order(
        Guid id,
        CustomerIdentity customerId,
        IReadOnlyCollection<OrderItem> items)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Order ID cannot be empty.", nameof(id));

        ArgumentNullException.ThrowIfNull(customerId);
        ArgumentNullException.ThrowIfNull(items);

        if (items.Count == 0)
            throw new InvalidOperationException("Order must contain at least one item.");

        Id = id;
        CustomerId = customerId;
        Status = OrderStatus.Pending;
        CreatedAt = DateTimeOffset.UtcNow;
        // Начальная версия
        Version = 1;

        _items.AddRange(items);

        // Все позиции одного заказа должны иметь одну валюту.
        //var currency = items
        //    .Select(x => x.UnitPrice.CurrencyCode)
        //    .Distinct(StringComparer.OrdinalIgnoreCase)
        //    .Single();
        var currency = items.First().UnitPrice.CurrencyCode;
        if (items.Any(x => !string.Equals(x.UnitPrice.CurrencyCode, currency, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException(
                "All order items must use the same currency.");

        var total = items.Sum(x => x.UnitPrice.Amount * x.Quantity.Value);

        TotalAmount = Money.Create(total, currency);

        //AddDomainEvent(
        //    new OrderCreatedDomainEvent(
        //        OrderId: Id,
        //        CustomerId: CustomerId,
        //        TotalAmount: TotalAmount,
        //        OccurredAt: CreatedAt));
    }

    /// <summary>
    /// Фабрика для создания нового заказа.
    /// </summary>
    public static Order Create(
        Guid id,
        CustomerIdentity customerId,
        IReadOnlyCollection<OrderItem> items)
        => new(id, customerId, items);

    ///// <summary>
    ///// Пример бизнес-метода изменения состояния агрегата.
    ///// Каждая модификация агрегата обязана инкрементировать версию.
    ///// </summary>
    //public void ChangeStatus(string newStatus)
    //{
    //    if (string.IsNullOrWhiteSpace(newStatus))
    //        throw new ArgumentException("Status cannot be empty.", nameof(newStatus));

    //    Status = newStatus;
    //    Version++;
    //}
}
