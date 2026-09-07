using Catalog.Domain.AggregatesModel.Products.Exceptions.Prices;

namespace Catalog.Domain.AggregatesModel.Products.ValueObjects;

/// <summary>
/// Value object для цены.
/// Хранит сумму и валюту. В Catalog всегда USD.
/// </summary>
public sealed record Price
{
    /// <summary>
    /// Сумма. Не может быть отрицательной.
    /// </summary>
    public decimal Amount { get; }

    /// <summary>
    /// Код валюты в формате ISO 4217 (3 буквы).
    /// </summary>
    public string CurrencyCode { get; }

    private Price(decimal amount, string currencyCode)
    {
        if (amount <= 0)
            throw new NegativePriceException(amount);

        if (!IsSupportedCurrency(currencyCode))
            throw new UnsupportedCurrencyException(currencyCode);

        Amount = amount;
        CurrencyCode = currencyCode?.ToUpperInvariant() ?? string.Empty;
    }

    public static Price Create(decimal amount, string currencyCode)
        => new(amount, currencyCode);

    private static bool IsSupportedCurrency(string code)
        // Точная проверка только USD — Catalog хранит цену в одной валюте.
        => code?.Equals("USD", StringComparison.OrdinalIgnoreCase) == true;
}