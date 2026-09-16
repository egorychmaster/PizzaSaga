using PizzaSaga.SharedKernel.Domain.Exceptions;

namespace Stock.Domain.AggregatesModel.Inventory.Exceptions;

/// <summary>
/// Исключение, возникающее при недостаточном доступном количестве товара для резервирования.
/// </summary>
public sealed class InsufficientAvailableQuantityException : DomainException
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="availableQuantity">Доступное количество товара (можно резервировать)</param>
    /// <param name="requestedQuantity">Количество для резервирования.</param>
    public InsufficientAvailableQuantityException(int availableQuantity, int requestedQuantity)
        : base($"Insufficient available quantity. Available: {availableQuantity}, Requested: {requestedQuantity}.")
    {
    }
}
