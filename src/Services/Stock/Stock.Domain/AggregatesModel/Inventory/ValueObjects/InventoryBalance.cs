using Stock.Domain.AggregatesModel.Inventory.Exceptions;

namespace Stock.Domain.AggregatesModel.Inventory.ValueObjects;

/// <summary>
/// Value Object, представляющий баланс инвентаря.
/// Инкапсулирует доступное и зарезервированное количество товара и правила их изменения.
/// </summary>
public sealed record InventoryBalance
{
    /// <summary>
    /// Доступное количество товара для резервирования.
    /// </summary>
    public Quantity Available { get; }

    /// <summary>
    /// Зарезервированное количество товара.
    /// </summary>
    public Quantity Reserved { get; }

    /// <summary>
    /// Общее количество товара: доступное плюс зарезервированное.
    /// </summary>
    public int Total => checked(Available.Value + Reserved.Value);

    private InventoryBalance(Quantity available, Quantity reserved)
    {
        Available = available;
        Reserved = reserved;
    }

    /// <summary>
    /// Создаёт баланс инвентаря.
    /// </summary>
    /// <param name="available">Доступное количество товара.</param>
    /// <param name="reserved">Зарезервированное количество товара.</param>
    /// <returns>Новый экземпляр InventoryBalance.</returns>
    public static InventoryBalance Create(int available, int reserved)
        => new(
            Quantity.Create(available),
            Quantity.Create(reserved));

    /// <summary>
    /// Возвращает пустой баланс инвентаря.
    /// </summary>
    public static InventoryBalance Zero => new(Quantity.Zero, Quantity.Zero);

    /// <summary>
    /// Резервирует указанное количество товара.
    /// Перемещает количество из доступного остатка в зарезервированный.
    /// </summary>
    /// <param name="quantity">Количество товара для резервирования.</param>
    /// <returns>Новый баланс инвентаря.</returns>
    public InventoryBalance Reserve(int quantity)
    {
        ValidateOperationQuantity(quantity);

        if (Available.Value < quantity)
            throw new InsufficientAvailableQuantityException(Available.Value, quantity);

        var requested = Quantity.Create(quantity);

        return new(
            Available.Subtract(requested),
            Reserved.Add(requested));
    }

    /// <summary>
    /// Освобождает ранее зарезервированное количество товара.
    /// Перемещает количество из зарезервированного остатка в доступный.
    /// </summary>
    /// <param name="quantity">Количество товара для освобождения.</param>
    /// <returns>Новый баланс инвентаря.</returns>
    public InventoryBalance Release(int quantity)
    {
        ValidateOperationQuantity(quantity);

        if (Reserved.Value < quantity)
            throw new InsufficientReservedQuantityException(Reserved.Value, quantity);

        var requested = Quantity.Create(quantity);

        return new(
            Available.Add(requested),
            Reserved.Subtract(requested));
    }

    /// <summary>
    /// Проверяет, достаточно ли доступного количества
    /// для резервирования указанного количества товара.
    /// </summary>
    /// <param name="quantity">Количество товара для резервирования.</param>
    /// <returns>true, если количество можно зарезервировать.</returns>
    public bool CanReserve(int quantity)
    {
        if (quantity <= 0)
            return false;

        return Available.Value >= quantity;
    }

    private static void ValidateOperationQuantity(int quantity)
    {
        if (quantity <= 0)
            throw new InvalidInventoryQuantityException(quantity);
    }
}