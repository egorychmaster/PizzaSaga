using PizzaSaga.SharedKernel.Domain;
using PizzaSaga.SharedKernel.Domain.ValueObjects;

namespace Payment.Domain.AggregatesModel.PaymentReservation;

/// <summary>
/// Агрегат платежной резервации.
/// Хранит информацию о резервировании средств для заказа.
/// </summary>
public sealed class PaymentReservationAggregate : AggregateRootWithId
{
    /// <summary>
    /// Идентификатор заказа.
    /// </summary>
    public Guid OrderId { get; private set; } = default!;

    /// <summary>
    /// Сумма к оплате и её валюта.
    /// </summary>
    public Money TotalAmount { get; private set; } = null!;

    /// <summary>
    /// Состояние платежа.
    /// </summary>
    public PaymentStatus Status { get; private set; }

    /// <summary>
    /// Версия для оптимистичной блокировки (Optimistic Concurrency).
    /// Инкрементируется при каждом изменении состояния агрегата.
    /// </summary>
    public int Version { get; private set; }

    /// <summary>
    /// Дата и время создания записи.
    /// </summary>
    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Дата и время последнего обновления.
    /// </summary>
    public DateTimeOffset? UpdatedAt { get; private set; }

    /// <summary>
    /// Конструктор для EF Core.
    /// </summary>
    private PaymentReservationAggregate() { }

    /// <summary>
    /// Создаёт новую запись платежной резервации.
    /// </summary>
    /// <param name="orderId">Идентификатор заказа.</param>
    /// <param name="amount">Сумма к оплате.</param>
    /// <param name="currencyCode">Код валюты (ISO 4217).</param>
    public static PaymentReservationAggregate Create(Guid orderId, decimal amount, string currencyCode)
    {
        var reservation = new PaymentReservationAggregate
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            TotalAmount = Money.Create(amount, currencyCode),
            Status = PaymentStatus.Pending,
            Version = 1
        };

        return reservation;
    }

    /// <summary>
    /// Обновляет статус резервации.
    /// </summary>
    /// <param name="newStatus">Новый статус.</param>
    public void UpdateStatus(PaymentStatus newStatus)
    {
        Status = newStatus;
        UpdatedAt = DateTimeOffset.UtcNow;
        Version++;
    }
}
