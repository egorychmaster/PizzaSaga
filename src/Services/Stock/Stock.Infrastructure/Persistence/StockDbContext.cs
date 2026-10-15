using MassTransit;
using Microsoft.EntityFrameworkCore;
using Stock.Domain.AggregatesModel.Inventory;
using Stock.Domain.AggregatesModel.InventoryReservation;

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

    /// <summary>
    /// Резервы инвентаря для заказов.
    /// </summary>
    public DbSet<InventoryReservationAggregate> InventoryReservations => Set<InventoryReservationAggregate>();

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
