namespace Order.Domain.AggregatesModel.Orders.ValueObjects;

/// <summary>
/// Курс конвертации валют. 
/// Entity с композитным PK (FromCurrencyCode, ToCurrencyCode).
/// По заданию FromCurrencyCode всегда "USD", но оставлено поле для гибкости.
/// </summary>
public sealed class CurrencyExchangeRate
{
    /// <summary>
    /// Код исходной валюты (обычно "USD").
    /// </summary>
    public string FromCurrencyCode { get; set; } = default!;

    /// <summary>
    /// Код целевой валюты (например, "RUB", "EUR").
    /// </summary>
    public string ToCurrencyCode { get; set; } = default!;

    /// <summary>
    /// Курс конвертации: сколько единиц ToCurrency в 1 единице FromCurrency.
    /// Например: ("USD", "RUB") → 108.5m означает, что 1 USD = 108.5 RUB.
    /// </summary>
    public decimal Rate { get; set; }
}
