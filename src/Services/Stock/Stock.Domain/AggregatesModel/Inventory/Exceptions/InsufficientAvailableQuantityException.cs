using PizzaSaga.SharedKernel.Domain.Exceptions;

namespace Stock.Domain.AggregatesModel.Inventory.Exceptions;

/// <summary>
/// Исключение, возникающее при недостаточном доступном количестве товара для резервирования.
/// </summary>
public sealed class InsufficientAvailableQuantityException : DomainException
{
    /// <summary>
    /// Идентификатор продукта, по которому не хватило остатка.
    /// </summary>
    public Guid? ProductId { get; }

    /// <summary>
    /// Доступное количество товара (можно резервировать).
    /// </summary>
    public int AvailableQuantity { get; }

    /// <summary>
    /// Запрошенное количество.
    /// </summary>
    public int RequestedQuantity { get; }

    /// <summary>
    /// Инициализирует новое исключение при недостатке доступного количества.
    /// </summary>
    /// <param name="productId">Идентификатор продукта.</param>
    /// <param name="availableQuantity">Доступное количество товара (можно резервировать)</param>
    /// <param name="requestedQuantity">Количество для резервирования.</param>
    public InsufficientAvailableQuantityException(Guid? productId, int availableQuantity, int requestedQuantity)
        : base($"Insufficient available quantity for ProductId={productId}. Available: {availableQuantity}, Requested: {requestedQuantity}.")
    {
        ProductId = productId;
        AvailableQuantity = availableQuantity;
        RequestedQuantity = requestedQuantity;
    }
}
