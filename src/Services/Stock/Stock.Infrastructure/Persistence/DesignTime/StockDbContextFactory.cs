using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Stock.Infrastructure.Persistence.DesignTime;

/// <summary>
/// Фабрика для создания DbContext во время выполнения EF Core design-time операций.
/// </summary>
public sealed class StockDbContextFactory : IDesignTimeDbContextFactory<StockDbContext>
{
    /// <summary>
    /// Создаёт экземпляр DbContext для EF Core migrations.
    /// </summary>
    public StockDbContext CreateDbContext(string[] args)
    {
        var connectionString = "Host=localhost;Port=5432;Database=Stock;Username=postgres;Password=";
        var optionsBuilder = new DbContextOptionsBuilder<StockDbContext>();

        optionsBuilder.UseNpgsql(
            connectionString,
            npgsqlOptions =>
            {
                npgsqlOptions.EnableRetryOnFailure();
            });

        return new StockDbContext(optionsBuilder.Options);
    }
}