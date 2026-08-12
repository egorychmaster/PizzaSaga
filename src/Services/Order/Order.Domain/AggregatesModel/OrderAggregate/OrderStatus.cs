namespace Order.Domain.AggregatesModel.OrderAggregate;

/// <summary>
/// Публичные статусы заказа.
/// </summary>
/// <remarks>
/// Эти значения являются частью доменной модели и соответствуют состояниям, возвращаемым внешнему API.
///
/// Внутренние состояния Saga преобразуются в данные статусы при завершении этапов бизнес-процесса.
/// </remarks>
public enum OrderStatus
{
    /// <summary>
    /// Заказ создан и находится в обработке.
    /// </summary>
    Pending = 1,

    /// <summary>
    /// Заказ успешно выполнен.
    /// </summary>
    Completed = 2,

    /// <summary>
    /// Заказ отменён.
    /// </summary>
    Cancelled = 3
}