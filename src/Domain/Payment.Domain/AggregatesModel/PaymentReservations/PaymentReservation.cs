namespace Payment.Domain.AggregatesModel.PaymentReservations;

/// <summary>
/// Сущность резервирования платежа.
/// Используется для отслеживания попыток авторизации и списания средств по заказу.
/// В Sprint 2 — минимальная реализация без статусов и дат.
/// </summary>
public sealed class PaymentReservation
{
    /// <summary>
    /// Уникальный идентификатор резервирования (новый GUID, не OrderId).
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Идентификатор заказа, к которому привязано резервирование.
    /// </summary>
    public Guid OrderId { get; set; }

    /// <summary>
    /// Сумма резервирования.
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Код валюты (например, "RUB", "USD").
    /// </summary>
    public string CurrencyCode { get; set; } = null!;

    /// <summary>
    /// Версия для оптимистичной блокировки.
    /// </summary>
    public int Version { get; set; }
}
