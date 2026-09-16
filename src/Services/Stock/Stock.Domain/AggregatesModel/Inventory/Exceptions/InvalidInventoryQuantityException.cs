using PizzaSaga.SharedKernel.Domain.Exceptions;

namespace Stock.Domain.AggregatesModel.Inventory.Exceptions;

/// <summary>
/// Исключение, возникающее при недопустимом значении количества (меньше или равно нулю).
/// </summary>
public sealed class InvalidInventoryQuantityException : DomainException
{
    public InvalidInventoryQuantityException(int quantity)
        : base($"Invalid inventory quantity. Value must be greater than zero. Actual: {quantity}.")
    {
    }
}
