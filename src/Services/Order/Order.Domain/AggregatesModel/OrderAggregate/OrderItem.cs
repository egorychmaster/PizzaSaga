using Order.Domain.ValueObjects;

namespace Order.Domain.AggregatesModel.OrderAggregate;

/// <summary>
/// Позиция заказа.
/// </summary>
public sealed class OrderItem
{
    /// <summary>
    /// Идентификатор позиции заказа.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Идентификатор продукта каталога.
    /// </summary>
    public Guid ProductId { get; private set; }

    /// <summary>
    /// Количество продукта.
    /// </summary>
    public PizzaQuantity Quantity { get; private set; } = null!;

    /// <summary>
    /// Зафиксированная цена одной единицы продукта на момент создания заказа.
    /// </summary>
    public Money UnitPrice { get; private set; } = null!;

    private OrderItem()
    {
    }

    /// <summary>
    /// Создаёт позицию заказа.
    /// </summary>
    public OrderItem(
        Guid id,
        Guid productId,
        PizzaQuantity quantity,
        Money unitPrice)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Order item ID cannot be empty.", nameof(id));

        if (productId == Guid.Empty)
            throw new ArgumentException("Product ID cannot be empty.", nameof(productId));

        ArgumentNullException.ThrowIfNull(quantity);
        ArgumentNullException.ThrowIfNull(unitPrice);

        Id = id;
        ProductId = productId;
        Quantity = quantity;
        UnitPrice = unitPrice;
    }
}