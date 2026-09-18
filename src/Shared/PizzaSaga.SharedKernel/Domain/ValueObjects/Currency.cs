using PizzaSaga.SharedKernel.Domain.Exceptions.Currencies;

namespace PizzaSaga.SharedKernel.Domain.ValueObjects;

/// <summary>
/// Value Object, представляющий валюту в формате ISO 4217.
/// Поддерживает только валюты, разрешённые доменной моделью.
/// </summary>
public sealed record Currency
{
    /// <summary>
    /// Американский доллар.
    /// </summary>
    public static readonly Currency USD = new("USD");

    /// <summary>
    /// Евро.
    /// </summary>
    public static readonly Currency EUR = new("EUR");

    /// <summary>
    /// Российский рубль.
    /// </summary>
    public static readonly Currency RUB = new("RUB");

    private static readonly HashSet<string> AllowedCodes =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "USD",
            "EUR",
            "RUB"
        };

    /// <summary>
    /// Код валюты в формате ISO 4217.
    /// </summary>
    public string Code { get; }

    private Currency(string code)
    {
        Code = code;
    }

    /// <summary>
    /// Создаёт валюту из ISO 4217 кода.
    /// </summary>
    /// <param name="code">ISO 4217 код валюты.</param>
    /// <returns>Валидный экземпляр Currency.</returns>
    /// <exception cref="InvalidCurrencyCodeException">
    /// Если код имеет некорректный формат.
    /// </exception>
    /// <exception cref="UnsupportedCurrencyException">
    /// Если валюта не поддерживается системой.
    /// </exception>
    public static Currency Create(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new InvalidCurrencyCodeException("Currency code cannot be null or empty.");

        if (code.Length != 3)
            throw new InvalidCurrencyCodeException($"Currency code must be exactly 3 characters. Received: '{code}'");

        if (!code.All(char.IsLetter))
            throw new InvalidCurrencyCodeException($"Currency code must contain only letters. Received: '{code}'");

        var upperCode = code.ToUpperInvariant();

        if (!AllowedCodes.Contains(upperCode))
            throw new UnsupportedCurrencyException(code);

        return upperCode switch
        {
            "USD" => USD,
            "EUR" => EUR,
            "RUB" => RUB,
            _ => throw new UnsupportedCurrencyException(code)
        };
    }

    /// <inheritdoc />
    public override string ToString() => Code;
}
