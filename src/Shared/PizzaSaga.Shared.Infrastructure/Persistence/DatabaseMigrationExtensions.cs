using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace PizzaSaga.Shared.Infrastructure.Persistence;

public static class DatabaseMigrationExtensions
{
    /// <summary>
    /// Автоматически проверяет и накатывает не применённые миграции EF Core до старта Kestrel,
    /// а также запускает делегат идемпотентного сидинга данных.
    /// </summary>
    /// <typeparam name="TContext">Тип контекста базы данных (DbContext).</typeparam>
    /// <param name="app">Экземпляр запущенного IHost.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    public static async Task ApplyMigrationsAsync<TContext>(
        this IHost app,
        CancellationToken cancellationToken = default)
        where TContext : DbContext
    {
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<TContext>>();
        var dbContext = services.GetRequiredService<TContext>();

        try
        {
            var contextName = typeof(TContext).Name;
            logger.LogInformation("Checking database state for {DbContext}...", contextName);

            var pendingMigrations = await dbContext.Database.GetPendingMigrationsAsync(cancellationToken);
            if (pendingMigrations.Any())
            {
                logger.LogInformation("Found {Count} pending migration(s). Applying database migrations...",
                    pendingMigrations.Count());

                await dbContext.Database.MigrateAsync(cancellationToken);

                logger.LogInformation("Database migrations successfully applied for {DbContext}.", contextName);
            }
            else
            {
                logger.LogInformation("Database {DbContext} is up to date.", contextName);
            }

            // Автоматически пытаемся получить зарегистрированный IDatabaseSeeder<TContext> из DI
            var seeder = services.GetRequiredService<IDatabaseSeeder<TContext>>();

            logger.LogInformation("Starting database seeder {SeederName} for {DbContext}...", seeder.GetType().Name, contextName);
            await seeder.SeedAsync(dbContext, cancellationToken);
            logger.LogInformation("Database seeding completed successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while applying database migrations or seeding data for {DbContext}.", typeof(TContext).Name);
            throw;
        }
    }
}