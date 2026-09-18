namespace Payment.Domain.AggregatesModel.PaymentReservation;

/// <summary>
/// Состояние платежной резервации.
/// </summary>
public enum PaymentStatus
{
    /// <summary>
    /// Резервация создана, ожидает авторизации.
    /// </summary>
    Pending,

    /// <summary>
    /// Средства успешно авторизованы (заблокированы).
    /// </summary>
    Authorized,

    /// <summary>
    /// Резервация отменена.
    /// </summary>
    Cancelled
}
