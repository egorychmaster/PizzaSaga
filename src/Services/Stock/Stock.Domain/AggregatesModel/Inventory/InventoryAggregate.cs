using PizzaSaga.SharedKernel.Domain;
using Stock.Domain.AggregatesModel.Inventory.ValueObjects;

namespace Stock.Domain.AggregatesModel.Inventory;

/// <summary>
/// Агрегат инвентаря (остатков) для продукта.
/// Хранит баланс доступного и зарезервированного количества товара.
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
    public int AvailableQuantity => Balance.Available.Value;

    /// <summary>
    /// Зарезервированное количество (заблокировано в заказах).
    /// </summary>
    public int ReservedQuantity => Balance.Reserved.Value;

    /// <summary>
    /// Баланс инвентаря: доступное и зарезервированное количество.
    /// </summary>
    public InventoryBalance Balance { get; private set; } = InventoryBalance.Zero;

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
            Balance = InventoryBalance.Create(availableQuantity, reserved: 0)
        };

        return inventory;
    }

    /// <summary>
    /// Резервирует указанное количество товара.
    /// </summary>
    /// <param name="quantity">Количество для резервирования.</param>
    public void Reserve(int quantity)
    {
        Balance = Balance.Reserve(quantity);
    }

    /// <summary>
    /// Освобождает ранее зарезервированное количество.
    /// </summary>
    /// <param name="quantity">Количество для освобождения.</param>
    public void Release(int quantity)
    {
        Balance = Balance.Release(quantity);
    }

    /// <summary>
    /// Проверяет, достаточно ли доступного количества для резервирования.
    /// </summary>
    /// <param name="quantity">Запрашиваемое количество.</param>
    public bool CanReserve(int quantity) => Balance.CanReserve(quantity);
}
