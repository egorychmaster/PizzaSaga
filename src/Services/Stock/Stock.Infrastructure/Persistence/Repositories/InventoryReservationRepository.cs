using Microsoft.EntityFrameworkCore;
using Stock.Domain.AggregatesModel.InventoryReservation;

namespace Stock.Infrastructure.Persistence.Repositories;

/// <summary>
/// Реализация репозитория для работы с резервами инвентаря.
/// </summary>
public sealed class InventoryReservationRepository : IInventoryReservationRepository
{
    private readonly StockDbContext _dbContext;

    /// <summary>
    /// Инициализирует новый экземпляр репозитория.
    /// </summary>
    /// <param name="dbContext">Контекст базы данных.</param>
    public InventoryReservationRepository(StockDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    /// <inheritdoc />
    public async Task<InventoryReservationAggregate?> FindByOrderIdAsync(Guid orderId, CancellationToken ct = default)
    {
        return await _dbContext.InventoryReservations
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.OrderId == orderId, ct);
    }

    /// <inheritdoc />
    public async Task AddAsync(InventoryReservationAggregate reservation, CancellationToken ct = default)
    {
        await _dbContext.InventoryReservations.AddAsync(reservation, ct);
    }
}
