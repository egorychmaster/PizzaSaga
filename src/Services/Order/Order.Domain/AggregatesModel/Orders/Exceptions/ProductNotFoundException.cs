namespace Order.Domain.AggregatesModel.Orders.Exceptions;

/// <summary>
/// Исключение, выбрасываемое при попытке создать заказ с продуктом, отсутствующим в локальном кэше.
/// </summary>
public sealed class ProductNotFoundException : Exception
{
    /// <summary>
    /// Идентификатор продукта, который не найден.
    /// </summary>
    public Guid ProductId { get; }

    /// <summary>
    /// Создаёт экземпляр исключения.
    /// </summary>
    /// <param name="productId">Идентификатор продукта.</param>
    public ProductNotFoundException(Guid productId)
        : base($"Product with ID '{productId}' not found in local cache.")
    {
        ProductId = productId;
    }
}
