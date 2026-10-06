using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PizzaSaga.Shared.Infrastructure.Messaging;
using Stock.Infrastructure.MassTransit.CommandConsumers;
using Stock.Infrastructure.MassTransit.EventConsumers;
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

        // Регистрируем UnitOfWork — реализация IUnitOfWork для EF Core.
        //services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Регистрируем сидер БД (хотя в данном случае seed не нужен — остатки создаются через consumer)
        //services.AddScoped<IDatabaseSeeder<StockDbContext>, StockDatabaseSeeder>();

        // Подключаем MassTransit с RabbitMQ, Outbox и consumer'ами
        services.AddMassTransit(x =>
        {
            // Регистрация потребителей из указанных сборок
            x.AddConsumers(typeof(ReserveInventoryConsumer).Assembly);

            // Включаем EF Core Transactional Outbox для надёжной публикации сообщений.
            // Сообщения будут сохраняться в таблицу OutboxMessage внутри той же транзакции, что и Inventory.
            // При коммите транзакции MassTransит отправит сообщения в RabbitMQ.
            x.AddEntityFrameworkOutbox<StockDbContext>(options =>
            {
                options.UsePostgres();
            });

            // Конфигурация RabbitMQ
            x.UsingRabbitMq((context, cfg) =>
            {
                var uri = new Uri(rabbitMqConnectionString);
                cfg.Host(uri);

                cfg.ReceiveEndpoint(RabbitMqQueues.StockReserveInventory,
                    endpoint =>
                    {
                        // Consumer Outbox: входящее сообщение, изменения БД и исходящие сообщения
                        // обрабатываются в рамках одной транзакционной границы.
                        endpoint.UseEntityFrameworkOutbox<StockDbContext>(context);

                        endpoint.ConfigureConsumer<ReserveInventoryConsumer>(context);
                    });

                cfg.ReceiveEndpoint(RabbitMqQueues.StockReleaseInventory,
                    endpoint =>
                    {
                        // Consumer Outbox: входящее сообщение, изменения БД и исходящие сообщения, обрабатываются в рамках одной транзакционной границы.
                        endpoint.UseEntityFrameworkOutbox<StockDbContext>(context);

                        endpoint.ConfigureConsumer<ReleaseInventoryConsumer>(context);
                    });

                cfg.ReceiveEndpoint(RabbitMqQueues.StockProductCreated,
                    endpoint =>
                    {
                        // Consumer Outbox: входящее сообщение, изменения БД и исходящие сообщения, обрабатываются в рамках одной транзакционной границы.
                        endpoint.UseEntityFrameworkOutbox<StockDbContext>(context);

                        endpoint.ConfigureConsumer<ProductCreatedConsumer>(context);
                    });

                //cfg.ConfigureEndpoints(context);
            });
        });

        return services;
    }
}
