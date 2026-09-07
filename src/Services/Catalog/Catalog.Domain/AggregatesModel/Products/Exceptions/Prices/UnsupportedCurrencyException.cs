using PizzaSaga.SharedKernel.Domain.Exceptions;

namespace Catalog.Domain.AggregatesModel.Products.Exceptions.Prices;

/// <summary>
/// Данная валюта не поддерживается.
/// </summary>
public sealed class UnsupportedCurrencyException : DomainException
{
    public string Code { get; }
    public UnsupportedCurrencyException(string code)
        : base($"Currency '{code}' is not supported or invalid.")
    {
        Code = code;
    }
}