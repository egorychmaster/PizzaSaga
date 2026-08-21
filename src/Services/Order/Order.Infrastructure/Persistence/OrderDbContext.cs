using Microsoft.EntityFrameworkCore;
using Order.Infrastructure.Persistence.Idempotency;
using OrderAggregate = Order.Domain.AggregatesModel.OrderAggregate.Order;

namespace Order.Infrastructure.Persistence;

public sealed class OrderDbContext : DbContext
{
    public OrderDbContext(DbContextOptions<OrderDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Сущности/агрегата Заказа.
    /// </summary>
    public DbSet<OrderAggregate> Orders => Set<OrderAggregate>();

    /// <summary>
    /// Набор записей идемпотентности HTTP-запросов.
    /// </summary>
    public DbSet<IdempotencyRecord> IdempotencyRecords => Set<IdempotencyRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Автоматическое применение всех конфигураций IEntityTypeConfiguration из текущей сборки (Order.Infrastructure)
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OrderDbContext).Assembly);
    }
}
