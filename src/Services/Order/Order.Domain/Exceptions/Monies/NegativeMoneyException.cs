using PizzaSaga.SharedKernel.Domain.Exceptions;

namespace Order.Domain.Exceptions.Monies;

/// <summary>
/// Исключение, возникающее при попытке создать денежное значение с отрицательной суммой.
/// </summary>
public sealed class NegativeMoneyException : DomainException
{
    public NegativeMoneyException(decimal amount)
        : base($"Money amount cannot be negative. Actual value: {amount}.")
    {
    }
}