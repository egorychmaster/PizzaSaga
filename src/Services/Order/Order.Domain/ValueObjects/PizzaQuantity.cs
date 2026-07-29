namespace Order.Domain.ValueObjects;

/// <summary>
/// Количество пиццы в заказе (от 1 до 10).
/// Исключает нулевые и отрицательные значения.
/// </summary>
public sealed class PizzaQuantity
{
    public const int MinQuantity = 1;
    public const int MaxQuantity = 10;

    /// <summary>
    /// Значение количества. Всегда от 1 до 10 включительно.
    /// </summary>
    public int Value { get; }

    private PizzaQuantity(int value)
    {

        if (value < MinQuantity || value > MaxQuantity)
            throw new ArgumentException($"Quantity must be between {MinQuantity} and {MaxQuantity}. Given: {value}");

        Value = value;
    }

    /// <summary>
    /// Фабричный метод для создания PizzaQuantity из целого числа.
    /// </summary>
    public static PizzaQuantity Create(int quantity) => new(quantity);

    /// <summary>
    /// Парсер: создаёт PizzaQuantity из int (валидирует диапазон).
    /// </summary>
    public static PizzaQuantity Parse(int rawValue) => Create(rawValue);
}