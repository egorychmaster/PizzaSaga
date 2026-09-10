using Catalog.Application.Abstractions.Persistence;
using Catalog.Infrastructure.Persistence;
using Catalog.Infrastructure.Persistence.Seeding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PizzaSaga.Shared.Infrastructure.DependencyInjection;
using PizzaSaga.Shared.Infrastructure.Persistence;

namespace Catalog.Infrastructure.DependencyInjection;

/// <summary>
/// Расширения для регистрации зависимостей слоя Infrastructure.
/// </summary>
public static class InfrastructureServiceCollectionExtensions
{
    /// <summary>
    /// Регистрирует зависимости инфраструктурного слоя.
    /// </summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string dbConnectionString, string rabbitMqConnectionString)
    {
        // Регистрируем context с настройками EF Core для PostgreSQL
        services.AddDbContext<CatalogDbContext>((sp, options) =>
        {
            // Используем Npgsql и стратегию повторных попыток (для transient ошибок)
            options.UseNpgsql(dbConnectionString, npgsqlOptions =>
            {
                // Включаем стратегию повторов: при ошибках (например, deadlock)
                // EF Core автоматически перезапустит транзакцию
                npgsqlOptions.EnableRetryOnFailure();
            });

            // Опционально: логирование SQL через ILogger из DI
            // options.UseLoggerFactory(sp.GetRequiredService<ILoggerFactory>());
        });

        // Подключаем MassTransit с RabbitMQ
        services.AddMassTransitWithRabbitMq(rabbitMqConnectionString);


        //services.AddScoped<ICatalogRepository, CatalogRepository>();

        // Регистрируем UnitOfWork — реализация IUnitOfWork для EF Core.
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Регистрируем сидер БД.
        // Он будет вызываться при старте приложения через DatabaseMigrationExtensions.ApplyMigrationsAsync<TContext>()
        services.AddScoped<IDatabaseSeeder<CatalogDbContext>, CatalogDatabaseSeeder>();

        return services;
    }
}