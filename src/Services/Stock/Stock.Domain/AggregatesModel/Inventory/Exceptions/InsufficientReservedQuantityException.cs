using PizzaSaga.SharedKernel.Domain.Exceptions;

namespace Stock.Domain.AggregatesModel.Inventory.Exceptions;

/// <summary>
/// Исключение, возникающее при недостаточном зарезервированном количестве товара для освобождения.
/// </summary>
public sealed class InsufficientReservedQuantityException : DomainException
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="reservedQuantity">Зарезервированное количество (заблокировано в заказах).</param>
    /// <param name="requestedQuantity">Количество для освобождения.</param>
    public InsufficientReservedQuantityException(int reservedQuantity, int requestedQuantity)
        : base($"Insufficient reserved quantity. Reserved: {reservedQuantity}, Requested: {requestedQuantity}.")
    {
    }
}
