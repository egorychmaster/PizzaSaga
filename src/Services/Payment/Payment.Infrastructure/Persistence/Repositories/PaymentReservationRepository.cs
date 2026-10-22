using Microsoft.EntityFrameworkCore;
using Payment.Domain.AggregatesModel.PaymentReservation;

namespace Payment.Infrastructure.Persistence.Repositories;

/// <summary>
/// Реализация репозитория для работы с платёжными резервациями.
/// </summary>
public sealed class PaymentReservationRepository : IPaymentReservationRepository
{
    private readonly PaymentDbContext _dbContext;

    /// <summary>
    /// Инициализирует новый экземпляр репозитория.
    /// </summary>
    /// <param name="dbContext">Контекст базы данных.</param>
    public PaymentReservationRepository(PaymentDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    /// <inheritdoc />
    public async Task<PaymentReservationAggregate?> FindByOrderIdAsync(Guid orderId, CancellationToken ct = default)
    {
        return await _dbContext.PaymentReservations
            .FirstOrDefaultAsync(x => x.OrderId == orderId, ct);
    }

    /// <inheritdoc />
    public async Task AddAsync(PaymentReservationAggregate reservation, CancellationToken ct = default)
    {
        await _dbContext.PaymentReservations.AddAsync(reservation, ct);
    }

    /// <summary>
    /// Сохраняет все изменения в базу данных.
    /// </summary>
    /// <param name="ct">Токен отмены.</param>
    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        return await _dbContext.SaveChangesAsync(ct);
    }
}
