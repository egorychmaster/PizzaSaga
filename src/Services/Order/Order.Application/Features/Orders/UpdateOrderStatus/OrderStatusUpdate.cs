namespace Order.Application.Features.Orders.UpdateOrderStatus;

/// <summary>
/// Допустимые терминальные изменения публичного статуса заказа.
/// Устанавливаются Saga после завершения распределённого бизнес-процесса.
/// </summary>
public enum OrderStatusUpdate
{
    /// <summary>
    /// Заказ успешно завершён.
    /// </summary>
    Completed = 1,

    /// <summary>
    /// Заказ отменён после выполнения компенсирующих операций.
    /// </summary>
    Cancelled = 2
}
