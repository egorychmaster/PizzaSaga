using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Order.Infrastructure.MassTransit.Saga;

namespace Order.Infrastructure.Persistence.DesignTime;

/// <summary>
/// Фабрика для создания OrderSagaDbContext во время выполнения EF Core design-time операций.
/// Используется для генерации миграций и обновления базы данных.
/// </summary>
public sealed class OrderSagaDbContextFactory : IDesignTimeDbContextFactory<OrderSagaDbContext>
{
    /// <summary>
    /// Создаёт экземпляр DbContext для EF Core migrations.
    /// </summary>
    public OrderSagaDbContext CreateDbContext(string[] args)
    {
        var connectionString = "Host=localhost;Port=5432;Database=orders;Username=postgres;Password=";
        var optionsBuilder = new DbContextOptionsBuilder<OrderSagaDbContext>();

        optionsBuilder.UseNpgsql(
            connectionString,
            npgsqlOptions =>
            {
                npgsqlOptions.EnableRetryOnFailure();
            });

        return new OrderSagaDbContext(optionsBuilder.Options);
    }
}