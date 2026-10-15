namespace Stock.Domain.AggregatesModel.InventoryReservation;

/// <summary>
/// Статус резерва инвентаря.
/// </summary>
public enum ReservationStatus
{
    /// <summary>
    /// Резерв успешно создан и активен.
    /// </summary>
    Reserved,

    /// <summary>
    /// Резерв освобожден (товар возвращен на склад).
    /// </summary>
    Released,
}
