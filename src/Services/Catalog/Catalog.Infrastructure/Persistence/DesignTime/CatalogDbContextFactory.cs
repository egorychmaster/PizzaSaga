using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Catalog.Infrastructure.Persistence.DesignTime;

/// <summary>
/// Фабрика для создания OrderDbContext во время выполнения EF Core design-time операций.
/// </summary>
public sealed class CatalogDbContextFactory : IDesignTimeDbContextFactory<CatalogDbContext>
{
    /// <summary>
    /// Создаёт экземпляр OrderDbContext для EF Core migrations.
    /// </summary>
    public CatalogDbContext CreateDbContext(string[] args)
    {
        var connectionString = "Host=localhost;Port=5432;Database=catalogs;Username=postgres;Password=";

        var optionsBuilder = new DbContextOptionsBuilder<CatalogDbContext>();

        optionsBuilder.UseNpgsql(
            connectionString,
            npgsqlOptions =>
            {
                npgsqlOptions.EnableRetryOnFailure();
            });

        return new CatalogDbContext(optionsBuilder.Options);
    }
}