using PizzaSaga.SharedKernel.Domain.Exceptions;

namespace Order.Domain.AggregatesModel.Orders.Exceptions.Currencies;

/// <summary>
/// Исключение, которое выбрасывается при попытке создать валюту с недопустимым форматом кода.
/// </summary>
public sealed class InvalidCurrencyCodeException : DomainException
{
    public InvalidCurrencyCodeException(string currencyCode)
        : base($"Invalid currency code format: '{currencyCode}'. Currency code must be a 3-letter ISO 4217 code.") { }
}