using PizzaSaga.SharedKernel.Domain.Exceptions;

namespace Order.Domain.Exceptions.Monies;

public sealed class UnsupportedCurrencyException : DomainException
{
    public UnsupportedCurrencyException(string code)
        : base($"Currency '{code}' is not supported or invalid.") { }
}