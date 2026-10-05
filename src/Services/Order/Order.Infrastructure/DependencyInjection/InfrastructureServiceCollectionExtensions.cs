using MassTransit;
using Microsoft.EntityFrameworkCore;
using PizzaSaga.Contracts.Payment.IntegrationCommands;
using PizzaSaga.Contracts.Stock.IntegrationCommands;
using Microsoft.Extensions.DependencyInjection;
using Order.Application.Abstractions.Persistence;
using Order.Application.Abstractions.Persistence.Idempotency;
using Order.Domain.Abstractions.Repositories;
using Order.Infrastructure.MassTransit.Consumers;
using Order.Infrastructure.MassTransit.Saga;
using Order.Infrastructure.Messaging;
using Order.Infrastructure.Persistence;
using Order.Infrastructure.Persistence.DomainEvents;
using Order.Infrastructure.Persistence.Idempotency;
using Order.Infrastructure.Persistence.Repositories;
using Order.Infrastructure.Persistence.Seeding;
using PizzaSaga.Shared.Infrastructure.Persistence.DomainEvents;
using PizzaSaga.Shared.Infrastructure.Persistence;
using PizzaSaga.SharedKernel.Domain.DomainEvents;
using PizzaSaga.SharedKernel.Messaging;

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
        // Регистрируем сидер БД.
        // Он будет вызываться при старте приложения через DatabaseMigrationExtensions.ApplyMigrationsAsync<TContext>()
        services.AddScoped<IDatabaseSeeder<OrderDbContext>, OrderDatabaseSeeder>();

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
        // Регистрация Idempotency Repository
        services.AddScoped<IIdempotencyRepository, IdempotencyRepository>();

        // Регистрируем UnitOfWork — реализация IUnitOfWork для EF Core.
        // Lifetime = Scoped (соответствует HTTP-запросу и DbContext).
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Регистрируем IDomainEventAccessor — позволяет получать доменные события из агрегатов.
        services.AddScoped<IDomainEventAccessor, EfCoreDomainEventAccessor<OrderDbContext>>();
        // Регистрируем IDomainEventDispatcher — публикует доменные события через Mediator.
        services.AddScoped<IDomainEventDispatcher, EfCoreDomainEventDispatcher>();


        // Регистрируем OrderSagaDbContext для хранения состояния Saga.
        // Использует ту же PostgreSQL базу данных, что и OrderDbContext.
        services.AddDbContext<OrderSagaDbContext>((sp, options) =>
        {
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.EnableRetryOnFailure();
            });
        });

        // Глобальные маршруты для команд, отправляемых из OrderStateMachine.
        // Должны быть зарегистрированы ДО AddMassTransit(...).
        EndpointConvention.Map<ReserveInventoryIntegrationCommand>(new Uri("queue:Stock-ReserveInventory"));
        EndpointConvention.Map<AuthorizePaymentIntegrationCommand>(new Uri("queue:Payment-AuthorizePayment"));

        // Подключаем MassTransit с RabbitMQ, Saga и EF Core Outbox.
        services.AddMassTransit(x =>
        {
            // Регистрация потребителей из указанных сборок
            x.AddConsumers(new[] { typeof(ProductCreatedIntegrationEventConsumer).Assembly });

            // Регистрация State Machine
            x.AddSagaStateMachine<OrderStateMachine, OrderSagaStateData>()
                .EntityFrameworkRepository(repository =>
                {
                    repository.ConcurrencyMode = ConcurrencyMode.Optimistic;
                    repository.ExistingDbContext<OrderSagaDbContext>();
                    repository.UsePostgres();
                });

            // Включаем EF Core Outbox для надёжной публикации сообщений.
            // Сообщения будут сохраняться в таблицу OutboxMessages внутри той же транзакции, что и Order/Aggregate.
            // При коммите транзакции MassTransit отправит сообщения в RabbitMQ.
            x.AddEntityFrameworkOutbox<OrderDbContext>(options =>
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
