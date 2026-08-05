using PizzaSaga.SharedKernel.Domain.Exceptions;

namespace Order.Domain.Exceptions.PizzaQuantities;

/// <summary>
/// Исключение, возникающее при попытке создать PizzaQuantity
/// со значением, выходящим за допустимый диапазон.
/// </summary>
public sealed class InvalidPizzaQuantityException : DomainException
{
    public InvalidPizzaQuantityException(int value, int min, int max)
        : base($"Pizza quantity must be between {min} and {max}. Actual value: {value}.")
    {
    }
}