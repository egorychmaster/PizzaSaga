using Order.Domain.AggregatesModel.Orders.Exceptions.Monies;

namespace Order.Domain.AggregatesModel.Orders.ValueObjects;

/// <summary>
/// Value Object, представляющий денежную сумму и её валюту.
/// Гарантирует неотрицательность суммы и валидность ISO-кода валюты.
/// </summary>
/// <summary>
public sealed class Money
{
    /// <summary>
    /// Денежная сумма.
    /// </summary>
    public decimal Amount { get; }

    /// <summary>
    /// Валюта денежной суммы.
    /// </summary>
    public Currency Currency { get; }

    private Money(decimal amount, Currency currency)
    {
        if (amount < 0)
            throw new NegativeMoneyException(amount);

        ArgumentNullException.ThrowIfNull(currency);

        Amount = amount;
        Currency = currency;
    }

    /// <summary>
    /// Создаёт денежное значение из суммы и ISO 4217 кода валюты.
    /// </summary>
    /// <param name="amount">Денежная сумма.</param>
    /// <param name="currencyCode">ISO 4217 код валюты.</param>
    /// <returns>Экземпляр Money.</returns>
    public static Money Create(decimal amount, string currencyCode)
        => new(amount, Currency.Create(currencyCode));

    /// <summary>
    /// Создаёт денежное значение из суммы и валюты.
    /// </summary>
    /// <param name="amount">Денежная сумма.</param>
    /// <param name="currency">Валюта.</param>
    /// <returns>Экземпляр Money.</returns>
    public static Money Create(decimal amount, Currency currency)
        => new(amount, currency);
}