using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Payment.Infrastructure.Persistence.DesignTime;

/// <summary>
/// Фабрика для создания DbContext во время выполнения EF Core design-time операций.
/// </summary>
public sealed class PaymentDbContextFactory : IDesignTimeDbContextFactory<PaymentDbContext>
{
    /// <summary>
    /// Создаёт экземпляр DbContext для EF Core migrations.
    /// </summary>
    public PaymentDbContext CreateDbContext(string[] args)
    {
        var connectionString = "Host=localhost;Port=5432;Database=Payment;Username=postgres;Password=";

        var optionsBuilder = new DbContextOptionsBuilder<PaymentDbContext>();

        optionsBuilder.UseNpgsql(
            connectionString,
            npgsqlOptions =>
            {
                npgsqlOptions.EnableRetryOnFailure();
            });

        return new PaymentDbContext(optionsBuilder.Options);
    }
}