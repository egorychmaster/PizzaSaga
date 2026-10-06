using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Payment.Infrastructure.MassTransit.CommandConsumers;
using Payment.Infrastructure.Persistence;

namespace Payment.Infrastructure.DependencyInjection;

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
        // Регистрируем PaymentDbContext с настройками EF Core для PostgreSQL
        services.AddDbContext<PaymentDbContext>((sp, options) =>
        {
            // Используем Npgsql и стратегию повторных попыток (для transient ошибок)
            options.UseNpgsql(dbConnectionString, npgsqlOptions =>
            {
                // Включаем стратегию повторов: при ошибках (например, deadlock)
                // EF Core автоматически перезапустит транзакцию
                npgsqlOptions.EnableRetryOnFailure();
            });
        });


        // Подключаем MassTransit с RabbitMQ, Outbox и consumer'ами.
        services.AddMassTransit(x =>
        {
            // Регистрация потребителей из указанной сборки.
            x.AddConsumers(typeof(AuthorizePaymentConsumer).Assembly);

            // Включаем EF Core Transactional Outbox для надёжной публикации сообщений.
            // Сообщения будут сохраняться в таблицу OutboxMessage внутри той же транзакции, что и PaymentReservation.
            // При коммите транзакции MassTransit отправит сообщения в RabbitMQ.
            x.AddEntityFrameworkOutbox<PaymentDbContext>(options =>
            {
                options.UsePostgres();
            });

            // Конфигурация RabbitMQ.
            x.UsingRabbitMq((context, cfg) =>
            {
                var uri = new Uri(rabbitMqConnectionString);
                cfg.Host(uri);

                cfg.ReceiveEndpoint("Payment-AuthorizePayment",
                    endpoint =>
                    {
                        // Consumer Outbox: входящее сообщение, изменения БД и исходящие сообщения
                        // обрабатываются в рамках одной транзакционной границы.
                        endpoint.UseEntityFrameworkOutbox<PaymentDbContext>(context);

                        endpoint.ConfigureConsumer<AuthorizePaymentConsumer>(context);
                    });

                
                //cfg.ConfigureEndpoints(context);
            });
        });

        return services;
    }
}
