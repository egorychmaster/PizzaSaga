using PizzaSaga.SharedKernel.Domain.Exceptions;

namespace Order.Domain.AggregatesModel.Orders.Exceptions;

/// <summary>
/// Исключение, выбрасываемое при попытке получить несуществующий заказ.
/// </summary>
public sealed class OrderNotFoundException : DomainException
{
    /// <summary>
    /// Идентификатор заказа, который не найден.
    /// </summary>
    public Guid OrderId { get; }

    /// <summary>
    /// Создаёт экземпляр исключения.
    /// </summary>
    /// <param name="orderId">Идентификатор заказа.</param>
    public OrderNotFoundException(Guid orderId)
        : base($"Order with ID '{orderId}' not found.")
    {
        OrderId = orderId;
    }
}
