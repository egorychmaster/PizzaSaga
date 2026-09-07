using PizzaSaga.SharedKernel.Domain.Exceptions;

namespace Catalog.Domain.AggregatesModel.Products.Exceptions.Prices;

/// <summary>
/// Исключение, возникающее при попытке создать значение цены с отрицательной суммой.
/// </summary>
public sealed class NegativePriceException : DomainException
{
    public decimal Amount { get; }
    public NegativePriceException(decimal amount)
        : base($"Price amount must be positive. Actual: {amount}")
    {
        Amount = amount;
    }
}
