using PizzaSaga.SharedKernel.Domain;
using Stock.Domain.AggregatesModel.InventoryReservation.Exceptions;

namespace Stock.Domain.AggregatesModel.InventoryReservation;

/// <summary>
/// Агрегат резерва инвентаря для одного заказа.
/// Содержит информацию о статусе резерва и перечень продуктов с количеством.
/// </summary>
public sealed class InventoryReservationAggregate : AggregateRootWithId
{
    /// <summary>
    /// Идентификатор заказа (уникален для одной операции резервирования).
    /// </summary>
    public Guid OrderId { get; private set; } = default!;

    /// <summary>
    /// Текущий статус резерва.
    /// </summary>
    public ReservationStatus Status { get; private set; }

    /// <summary>
    /// Время создания резерва.
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// Время последнего обновления статуса.
    /// </summary>
    public DateTime? UpdatedAt { get; private set; }

    /// <summary>
    /// Коллекция позиций резерва (продукты и их количества).
    /// </summary>
    public ICollection<InventoryReservationItem> Items { get; private set; } = new List<InventoryReservationItem>();

    /// <summary>
    /// Конструктор для EF Core.
    /// </summary>
    private InventoryReservationAggregate() { }

    /// <summary>
    /// Создает новый резерв для заказа.
    /// </summary>
    /// <param name="orderId">Идентификатор заказа.</param>
    /// <param name="items">Коллекция позиций резерва.</param>
    public static InventoryReservationAggregate Create(Guid orderId, IEnumerable<InventoryReservationItem> items)
    {
        var reservation = new InventoryReservationAggregate
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            Status = ReservationStatus.Reserved,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = null,
            Items = items.ToList()
        };

        return reservation;
    }

    /// <summary>
    /// Освобождает резерв (товар возвращается на склад).
    /// </summary>
    public void Release()
    {
        if (Status != ReservationStatus.Reserved)
            throw new InvalidReservationStateException($"Cannot release a reservation with status '{Status}'. Valid status: 'Reserved'.");

        Status = ReservationStatus.Released;
        UpdatedAt = DateTime.UtcNow;
    }
}
