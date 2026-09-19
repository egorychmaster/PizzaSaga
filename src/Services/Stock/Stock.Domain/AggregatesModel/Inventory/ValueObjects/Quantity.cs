using Stock.Domain.AggregatesModel.Inventory.Exceptions;

namespace Stock.Domain.AggregatesModel.Inventory.ValueObjects;

/// <summary>
/// Value Object, представляющий неотрицательное количество товара.
/// </summary>
public readonly record struct Quantity
{
    /// <summary>
    /// Значение количества.
    /// </summary>
    public int Value { get; }

    private Quantity(int value)
    {
        if (value < 0)
            throw new InvalidInventoryQuantityException(value);

        Value = value;
    }

    /// <summary>
    /// Создаёт количество из целочисленного значения.
    /// </summary>
    /// <param name="value">Количество товара.</param>
    /// <returns>Экземпляр Quantity.</returns>
    public static Quantity Create(int value) => new(value);

    /// <summary>
    /// Возвращает нулевое количество.
    /// </summary>
    public static Quantity Zero => new(0);

    /// <summary>
    /// Увеличивает количество на указанное значение.
    /// </summary>
    /// <param name="other">Количество для добавления.</param>
    /// <returns>Новое количество.</returns>
    public Quantity Add(Quantity other)
        => new(checked(Value + other.Value));

    /// <summary>
    /// Уменьшает количество на указанное значение.
    /// </summary>
    /// <param name="other">Количество для вычитания.</param>
    /// <returns>Новое количество.</returns>
    public Quantity Subtract(Quantity other)
        => new(checked(Value - other.Value));
}