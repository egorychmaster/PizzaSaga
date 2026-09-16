using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PizzaSaga.Shared.Infrastructure.DependencyInjection;
using Stock.Infrastructure.MassTransit.Consumers;
using Stock.Infrastructure.Persistence;

namespace Stock.Infrastructure.DependencyInjection;

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
        // Регистрируем StockDbContext с настройками EF Core для PostgreSQL
        services.AddDbContext<StockDbContext>((sp, options) =>
        {
            options.UseNpgsql(dbConnectionString, npgsqlOptions =>
            {
                npgsqlOptions.EnableRetryOnFailure();
            });
        });

        // Подключаем MassTransit с RabbitMQ и указываем сборку consumer'ов
        services.AddMassTransitWithRabbitMq(rabbitMqConnectionString, typeof(ProductCreatedIntegrationEventConsumer).Assembly);

        // Регистрируем UnitOfWork — реализация IUnitOfWork для EF Core.
        //services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Регистрируем сидер БД (хотя в данном случае seed не нужен — остатки создаются через consumer)
        //services.AddScoped<IDatabaseSeeder<StockDbContext>, StockDatabaseSeeder>();

        return services;
    }
}
