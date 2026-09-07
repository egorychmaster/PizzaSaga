using PizzaSaga.SharedKernel.Domain.Exceptions;

namespace Order.Domain.AggregatesModel.Orders.Exceptions.Monies;

public sealed class MoneyParsingException : DomainException
{
    public MoneyParsingException(string rawAmount)
        : base($"Invalid money amount format: '{rawAmount}'. Could not parse as decimal.") { }
}