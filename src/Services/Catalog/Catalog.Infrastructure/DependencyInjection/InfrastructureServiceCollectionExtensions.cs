using Catalog.Application.Abstractions.DomainEvents;
using Catalog.Application.Abstractions.Messaging;
using Catalog.Application.Abstractions.Persistence;
using Catalog.Infrastructure.Messaging;
using Catalog.Infrastructure.Persistence;
using Catalog.Infrastructure.Persistence.DomainEvents;
using Catalog.Infrastructure.Persistence.Seeding;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
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

        // Регистрируем UnitOfWork — реализация IUnitOfWork для EF Core.
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Регистрируем IDomainEventAccessor — позволяет получать доменные события из агрегатов.
        services.AddScoped<IDomainEventAccessor, EfCoreDomainEventAccessor>();
        // Регистрируем IDomainEventDispatcher — публикует доменные события через Mediator.
        services.AddScoped<IDomainEventDispatcher, EfCoreDomainEventDispatcher>();


        // Регистрируем сидер БД.
        // Он будет вызываться при старте приложения через DatabaseMigrationExtensions.ApplyMigrationsAsync<TContext>()
        services.AddScoped<IDatabaseSeeder<CatalogDbContext>, CatalogDatabaseSeeder>();


        // Подключаем MassTransit с RabbitMQ и EF Core Outbox.
        services.AddMassTransit(x =>
        {
            // Включает EF Core Outbox для надёжной публикации интеграционных событий.
            // Сообщения сохраняются в таблицу OutboxMessages внутри той же транзакции, что и агрегат.
            x.AddEntityFrameworkOutbox<CatalogDbContext>(options =>
            {
                options.UsePostgres();
                options.UseBusOutbox();
            });

            // Конфигурация RabbitMQ
            x.UsingRabbitMq((context, cfg) =>
            {
                var uri = new Uri(rabbitMqConnectionString);
                cfg.Host(uri);
                cfg.ConfigureEndpoints(context);
            });
        });

        // Регистрация IIntegrationEventPublisher через MassTransit реализацию.
        services.AddScoped<IIntegrationEventPublisher, MassTransitIntegrationEventPublisher>();

        return services;
    }
}