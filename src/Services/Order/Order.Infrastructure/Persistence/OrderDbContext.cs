using Microsoft.EntityFrameworkCore;
using OrderAggregate = Order.Domain.AggregatesModel.OrderAggregate.Order;

namespace Order.Infrastructure.Persistence;

public sealed class OrderDbContext : DbContext
{
    public OrderDbContext(DbContextOptions<OrderDbContext> options)
        : base(options)
    {
    }

    // DbSet сущности/агрегата Заказа
    public DbSet<OrderAggregate> Orders => Set<OrderAggregate>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Автоматическое применение всех конфигураций IEntityTypeConfiguration из текущей сборки (Order.Infrastructure)
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OrderDbContext).Assembly);
    }
}
