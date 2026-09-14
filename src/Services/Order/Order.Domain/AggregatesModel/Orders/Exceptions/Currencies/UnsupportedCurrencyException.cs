using PizzaSaga.SharedKernel.Domain.Exceptions;

namespace Order.Domain.AggregatesModel.Orders.Exceptions.Currencies;

/// <summary>
/// Данная валюта не поддерживается.
/// </summary>
public sealed class UnsupportedCurrencyException : DomainException
{
    public UnsupportedCurrencyException(string code)
        : base($"Currency '{code}' is not supported or invalid.") { }
}