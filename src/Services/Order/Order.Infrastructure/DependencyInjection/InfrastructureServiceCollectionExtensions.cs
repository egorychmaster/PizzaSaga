using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Order.Application.Abstractions.Persistence;
using Order.Application.Abstractions.Persistence.Idempotency;
using Order.Domain.Abstractions.Repositories;
using Order.Infrastructure.MassTransit.Consumers;
using Order.Infrastructure.Persistence;
using Order.Infrastructure.Persistence.Idempotency;
using Order.Infrastructure.Persistence.Repositories;
using Order.Infrastructure.Persistence.Seeding;
using PizzaSaga.Shared.Infrastructure.DependencyInjection;
using PizzaSaga.Shared.Infrastructure.Persistence;

namespace Order.Infrastructure.DependencyInjection;

/// <summary>
/// Расширения для регистрации зависимостей слоя Order.Infrastructure.
/// </summary>
public static class InfrastructureServiceCollectionExtensions
{
    /// <summary>
    /// Регистрирует зависимости инфраструктурного слоя.
    /// </summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString, string rabbitMqConnectionString)
    {
        // Регистрируем OrderDbContext с настройками EF Core для PostgreSQL
        services.AddDbContext<OrderDbContext>((sp, options) =>
        {
            // Используем Npgsql и стратегию повторных попыток (для transient ошибок)
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                // Включаем стратегию повторов: при ошибках (например, deadlock)
                // EF Core автоматически перезапустит транзакцию
                npgsqlOptions.EnableRetryOnFailure();
            });

            // Опционально: логирование SQL через ILogger из DI
            // options.UseLoggerFactory(sp.GetRequiredService<ILoggerFactory>());
        });

        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IProductCatalogRepository, ProductCatalogRepository>();
        services.AddScoped<ICurrencyExchangeRateRepository, CurrencyExchangeRateRepository>();

        // Регистрируем UnitOfWork — реализация IUnitOfWork для EF Core.
        // Lifetime = Scoped (соответствует HTTP-запросу и DbContext).
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Регистрируем сидер БД.
        // Он будет вызываться при старте приложения через DatabaseMigrationExtensions.ApplyMigrationsAsync<TContext>()
        services.AddScoped<IDatabaseSeeder<OrderDbContext>, OrderDatabaseSeeder>();

        // Регистрация Idempotency Repository
        services.AddScoped<IIdempotencyRepository, IdempotencyRepository>();

        // Подключаем MassTransit с RabbitMQ
        services.AddMassTransitWithRabbitMq(rabbitMqConnectionString, typeof(ProductCreatedIntegrationEventConsumer).Assembly);

        return services;
    }
}