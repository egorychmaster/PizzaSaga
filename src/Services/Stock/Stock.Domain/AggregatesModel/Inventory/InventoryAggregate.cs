using PizzaSaga.SharedKernel.Domain;
using Stock.Domain.AggregatesModel.Inventory.Exceptions;

namespace Stock.Domain.AggregatesModel.Inventory;

/// <summary>
/// Сущность инвентаря (остатков) для продукта.
/// Хранит доступное и зарезервированное количество товара.
/// </summary>
public sealed class InventoryAggregate : AggregateRootWithId
{
    /// <summary>
    /// Идентификатор продукта (PK).
    /// </summary>
    public Guid ProductId { get; private set; } = default!;

    /// <summary>
    /// Доступное количество товара (можно резервировать).
    /// </summary>
    public int AvailableQuantity { get; private set; }

    /// <summary>
    /// Зарезервированное количество (заблокировано в заказах).
    /// </summary>
    public int ReservedQuantity { get; private set; }

    /// <summary>
    /// Конструктор для EF Core.
    /// </summary>
    private InventoryAggregate() { }

    /// <summary>
    /// Создаёт новую запись инвентаря.
    /// </summary>
    /// <param name="productId">Идентификатор продукта.</param>
    /// <param name="availableQuantity">Начальный доступный остаток.</param>
    public static InventoryAggregate Create(Guid productId, int availableQuantity)
    {
        var inventory = new InventoryAggregate
        {
            Id = productId, // ProductId используется как PK
            ProductId = productId,
            AvailableQuantity = availableQuantity,
            ReservedQuantity = 0
        };

        return inventory;
    }

    /// <summary>
    /// Резервирует указанное количество товара.
    /// </summary>
    /// <param name="quantity">Количество для резервирования.</param>
    public void Reserve(int quantity)
    {
        if (quantity <= 0)
            throw new InvalidInventoryQuantityException(quantity);

        if (AvailableQuantity < quantity)
            throw new InsufficientAvailableQuantityException(AvailableQuantity, quantity);

        AvailableQuantity -= quantity;
        ReservedQuantity += quantity;
    }

    /// <summary>
    /// Освобождает ранее зарезервированное количество.
    /// </summary>
    /// <param name="quantity">Количество для освобождения.</param>
    public void Release(int quantity)
    {
        if (quantity <= 0)
            throw new InvalidInventoryQuantityException(quantity);

        if (ReservedQuantity < quantity)
            throw new InsufficientReservedQuantityException(ReservedQuantity, quantity);

        ReservedQuantity -= quantity;
        AvailableQuantity += quantity;
    }

    /// <summary>
    /// Проверяет, достаточно ли доступного количества для резервирования.
    /// </summary>
    /// <param name="quantity">Запрашиваемое количество.</param>
    public bool CanReserve(int quantity) => AvailableQuantity >= quantity;
}
