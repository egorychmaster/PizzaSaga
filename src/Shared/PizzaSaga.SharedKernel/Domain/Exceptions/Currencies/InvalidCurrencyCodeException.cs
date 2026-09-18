namespace PizzaSaga.SharedKernel.Domain.Exceptions.Currencies;

/// <summary>
/// Исключение для некорректного кода валюты.
/// </summary>
public sealed class InvalidCurrencyCodeException : DomainException
{
    /// <inheritdoc />
    public InvalidCurrencyCodeException(string message) 
        : base(message) { }
}
