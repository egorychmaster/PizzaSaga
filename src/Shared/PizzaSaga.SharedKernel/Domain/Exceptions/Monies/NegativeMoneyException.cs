namespace PizzaSaga.SharedKernel.Domain.Exceptions.Monies;

/// <summary>
/// Исключение для отрицательной денежной суммы.
/// </summary>
public sealed class NegativeMoneyException : DomainException
{
    /// <inheritdoc />
    public NegativeMoneyException(decimal amount) 
        : base($"Money amount cannot be negative. Actual value: {amount}.") { }
}
