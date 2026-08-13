using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Order.Infrastructure.Persistence.DesignTime;

/// <summary>
/// Фабрика для создания OrderDbContext во время выполнения EF Core design-time операций.
/// </summary>
public sealed class OrderDbContextFactory : IDesignTimeDbContextFactory<OrderDbContext>
{
    /// <summary>
    /// Создаёт экземпляр OrderDbContext для EF Core migrations.
    /// </summary>
    public OrderDbContext CreateDbContext(string[] args)
    {
        var connectionString = "Host=localhost;Port=5432;Database=orders;Username=postgres;Password=";
        var optionsBuilder = new DbContextOptionsBuilder<OrderDbContext>();

        optionsBuilder.UseNpgsql(
            connectionString,
            npgsqlOptions =>
            {
                npgsqlOptions.EnableRetryOnFailure();
            });

        return new OrderDbContext(optionsBuilder.Options);
    }
}