using MassTransit;
using Microsoft.EntityFrameworkCore;
using Stock.Domain.AggregatesModel.Inventory;

namespace Stock.Infrastructure.Persistence;

/// <summary>
/// Контекст базы данных для Stock Service.
/// </summary>
public class StockDbContext : DbContext
{
    /// <summary>
    /// Инвентарь (остатки) продуктов.
    /// </summary>
    public DbSet<InventoryAggregate> Inventories => Set<InventoryAggregate>();

    public StockDbContext(DbContextOptions<StockDbContext> options)
        : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Автоматическое применение всех конфигураций IEntityTypeConfiguration из текущей сборки.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(StockDbContext).Assembly);

        // Регистрирует сущности Outbox/Inbox MassTransit в модели EF Core
        modelBuilder.AddTransactionalOutboxEntities();
    }
}
