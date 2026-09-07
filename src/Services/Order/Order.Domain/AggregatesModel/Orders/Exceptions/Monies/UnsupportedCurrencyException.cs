using PizzaSaga.SharedKernel.Domain.Exceptions;

namespace Order.Domain.AggregatesModel.Orders.Exceptions.Monies;

public sealed class UnsupportedCurrencyException : DomainException
{
    public UnsupportedCurrencyException(string code)
        : base($"Currency '{code}' is not supported or invalid.") { }
}