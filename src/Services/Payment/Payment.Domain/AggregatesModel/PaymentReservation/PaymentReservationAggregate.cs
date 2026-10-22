using PizzaSaga.SharedKernel.Domain;
using PizzaSaga.SharedKernel.Domain.ValueObjects;

namespace Payment.Domain.AggregatesModel.PaymentReservation;

/// <summary>
/// Агрегат платёжной резервации.
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
    /// Версия строки для оптимистичной блокировки (Optimistic Concurrency).
    /// В PostgreSQL маппится на системный столбец xmin (row version).
    /// EF Core обновляет это поле автоматически при сохранении изменений.
    /// </summary>
    public uint Version { get; private set; }

    /// <summary>
    /// Дата и время создания записи.
    /// </summary>
    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Дата и время последнего обновления.
    /// </summary>
    public DateTimeOffset? UpdatedAt { get; private set; }

    /// <summary>
    /// Причина отказа (если статус Failed).
    /// Для терминальных состояний сохраняется для воспроизведения результата.
    /// </summary>
    public string? FailureReason { get; private set; }

    /// <summary>
    /// Конструктор для EF Core.
    /// </summary>
    private PaymentReservationAggregate() { }

    /// <summary>
    /// Создаёт новую запись платёжной резервации.
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
            Status = PaymentStatus.Pending
        };

        return reservation;
    }

    /// <summary>
    /// Авторизует платёж (Pending → Authorized).
    /// </summary>
    public void Authorize()
    {
        if (Status != PaymentStatus.Pending)
            throw new InvalidOperationException($"Cannot authorize payment with status '{Status}'. Valid status: 'Pending'.");

        Status = PaymentStatus.Authorized;
        FailureReason = null;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Отказывает в авторизации (Pending/Authorized → Failed).
    /// </summary>
    /// <param name="reason">Причина отказа.</param>
    public void Fail(string reason)
    {
        if (Status is PaymentStatus.Authorized or PaymentStatus.Cancelled)
            throw new InvalidOperationException($"Cannot fail payment with status '{Status}'. Valid statuses: 'Pending', 'Failed'.");

        Status = PaymentStatus.Failed;
        FailureReason = reason ?? "Payment authorization failed";
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Отменяет платёж (Pending/Authorized → Cancelled).
    /// </summary>
    public void Cancel()
    {
        if (Status is PaymentStatus.Failed or PaymentStatus.Cancelled)
            throw new InvalidOperationException($"Cannot cancel payment with status '{Status}'. Valid statuses: 'Pending', 'Cancelled'.");

        Status = PaymentStatus.Cancelled;
        FailureReason = "Payment cancelled by request";
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Определяет, является ли текущий статус терминальным (Authorized / Failed / Cancelled).
    /// Терминальные состояния не требуют повторной обработки команды авторизации.
    /// </summary>
    public bool IsTerminal() => Status is PaymentStatus.Authorized or PaymentStatus.Failed or PaymentStatus.Cancelled;

    /// <summary>
    /// Определяет, можно ли повторить операцию авторизации для текущего статуса.
    /// Повтор возможен только если операция ещё в состоянии Pending.
    /// </summary>
    public bool CanRetry() => Status == PaymentStatus.Pending;
}
