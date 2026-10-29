namespace Stock.Domain.AggregatesModel.InventoryReservation;

/// <summary>
/// Репозиторий для работы с резервами инвентаря.
/// </summary>
public interface IInventoryReservationRepository
{
    /// <summary>
    /// Находит резерв по идентификатору заказа.
    /// </summary>
    /// <param name="orderId">Идентификатор заказа.</param>
    /// <param name="ct">Токен отмены.</param>
    /// <returns>Экземпляр резерва или null, если не найден.</returns>
    Task<InventoryReservationAggregate?> FindByOrderIdAsync(Guid orderId, CancellationToken ct = default);

    /// <summary>
    /// Добавляет новый резерв.
    /// </summary>
    /// <param name="reservation">Резерв для добавления.</param>
    /// <param name="ct">Токен отмены.</param>
    Task AddAsync(InventoryReservationAggregate reservation, CancellationToken ct = default);
}
