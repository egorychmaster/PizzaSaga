namespace Payment.Domain.AggregatesModel.PaymentReservation;

/// <summary>
/// Репозиторий для работы с платёжными резервациями.
/// </summary>
public interface IPaymentReservationRepository
{
    /// <summary>
    /// Находит платёжную резервацию по идентификатору заказа.
    /// </summary>
    /// <param name="orderId">Идентификатор заказа.</param>
    /// <param name="ct">Токен отмены.</param>
    /// <returns>Экземпляр резервации или null, если не найдена.</returns>
    Task<PaymentReservationAggregate?> FindByOrderIdAsync(Guid orderId, CancellationToken ct = default);

    /// <summary>
    /// Добавляет новую платёжную резервацию.
    /// </summary>
    /// <param name="reservation">Резервация для добавления.</param>
    /// <param name="ct">Токен отмены.</param>
    Task AddAsync(PaymentReservationAggregate reservation, CancellationToken ct = default);

    /// <summary>
    /// Сохраняет все изменения в базу данных.
    /// </summary>
    /// <param name="ct">Токен отмены.</param>
    /// <returns>Количество сохранённых записей.</returns>
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
