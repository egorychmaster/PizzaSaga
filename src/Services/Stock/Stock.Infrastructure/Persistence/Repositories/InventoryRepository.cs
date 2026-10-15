using Microsoft.EntityFrameworkCore;
using Stock.Domain.AggregatesModel.Inventory;

namespace Stock.Infrastructure.Persistence.Repositories;

/// <summary>
/// Реализация репозитория для работы с инвентарем (остатками).
/// </summary>
public sealed class InventoryRepository : IInventoryRepository
{
    private readonly StockDbContext _dbContext;

    /// <summary>
    /// Инициализирует новый экземпляр репозитория.
    /// </summary>
    /// <param name="dbContext">Контекст базы данных.</param>
    public InventoryRepository(StockDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    /// <inheritdoc />
    public async Task<InventoryAggregate?> GetByIdAsync(Guid productId, CancellationToken ct = default)
    {
        return await _dbContext.Inventories.FindAsync(productId, ct);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<InventoryAggregate>> GetByIdsAsync(Guid[] productIds, CancellationToken ct = default)
    {
        return await _dbContext.Inventories
            .Where(x => productIds.Contains(x.ProductId))
            .ToListAsync(ct);
    }

    /// <inheritdoc />
    public async Task AddAsync(InventoryAggregate inventory, CancellationToken ct = default)
    {
        await _dbContext.Inventories.AddAsync(inventory, ct);
    }

    /// <inheritdoc />
    public async Task UpdateAsync(InventoryAggregate inventory, CancellationToken ct = default)
    {
        _dbContext.Inventories.Update(inventory);
    }
}
