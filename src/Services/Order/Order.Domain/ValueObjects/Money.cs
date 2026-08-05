using Order.Domain.Exceptions.Monies;
using System.Globalization;

namespace Order.Domain.ValueObjects;

/// <summary>
/// Значение денежной суммы с указанием валюты.
/// Гарантирует неотрицательность суммы и валидность ISO-кода валюты.
/// </summary>
public sealed class Money
{
    private static readonly HashSet<string> AllowedCurrencies = new(StringComparer.OrdinalIgnoreCase)
    {
        "EUR", "USD", "RUB"
    };

    /// <summary>
    /// Сумма. Не может быть отрицательной.
    /// </summary>
    public decimal Amount { get; }

    /// <summary>
    /// Код валюты в формате ISO 4217 (3 буквы).
    /// </summary>
    public string CurrencyCode { get; }

    private Money(decimal amount, string currencyCode)
    {
        if (amount < 0)
            throw new NegativeMoneyException(amount);

        if (!IsValidCurrencyCode(currencyCode))
            throw new InvalidCurrencyCodeException(currencyCode);

        if (!AllowedCurrencies.Contains(currencyCode))
            throw new UnsupportedCurrencyException(currencyCode);

        Amount = amount;
        CurrencyCode = currencyCode.ToUpperInvariant();
    }

    /// <summary>
    /// Фабричный метод для создания Money из числа и кода валюты.
    /// </summary>
    public static Money Create(decimal amount, string currencyCode)
        => new(amount, currencyCode);

    /// <summary>
    /// Парсер: создаёт Money из строкового представления суммы и кода валюты.
    /// </summary>
    public static Money Parse(string rawAmount, string currencyCode)
    {
        if (!decimal.TryParse(rawAmount, NumberStyles.Any, CultureInfo.InvariantCulture, out var amount))
            throw new MoneyParsingException(rawAmount);

        return Create(amount, currencyCode);
    }

    private static bool IsValidCurrencyCode(string code)
        => !string.IsNullOrWhiteSpace(code) &&
            code.Length == 3 &&
            code.ToUpperInvariant().All(char.IsLetter);
}