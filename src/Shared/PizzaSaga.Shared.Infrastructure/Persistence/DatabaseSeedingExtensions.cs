using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace PizzaSaga.Shared.Infrastructure.Persistence;

/// <summary>
/// Расширения для выполнения инициализации данных в базе.
/// </summary>
public static class DatabaseSeedingExtensions
{
    /// <summary>
    /// Выполняет идемпотентную инициализацию данных в базе с помощью зарегистрированного IDatabaseSeeder.
    /// </summary>
    /// <typeparam name="TContext">Тип контекста базы данных (DbContext).</typeparam>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    public static async Task SeedDatabaseAsync<TContext>(
        this IHost app,
        CancellationToken cancellationToken = default)
        where TContext : DbContext
    {
        // Получаем зарегистрированный IDatabaseSeeder<TContext> из DI-контейнера
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<TContext>>();
        var dbContext = services.GetRequiredService<TContext>();        
        var seeder = scope.ServiceProvider.GetRequiredService<IDatabaseSeeder<TContext>>();

        try
        {
            var contextName = typeof(TContext).Name;
            logger.LogInformation("Starting database seeder for {DbContext}...", contextName);

            logger.LogInformation("Executing database seeder {SeederName}...", seeder.GetType().Name);
            await seeder.SeedAsync(dbContext, cancellationToken);
            logger.LogInformation("Database seeding completed successfully for {DbContext}.", contextName);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while applying database migrations or seeding data for {DbContext}.", typeof(TContext).Name);
            throw;
        }
    }
}
