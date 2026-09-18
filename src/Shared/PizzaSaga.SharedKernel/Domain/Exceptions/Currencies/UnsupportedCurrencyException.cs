namespace PizzaSaga.SharedKernel.Domain.Exceptions.Currencies;

/// <summary>
/// Исключение для неподдерживаемой валюты.
/// </summary>
public sealed class UnsupportedCurrencyException : DomainException
{
    /// <inheritdoc />
    public UnsupportedCurrencyException(string code) 
        : base($"Unsupported currency code: '{code}'.") { }
}
