using MassTransit;
using Microsoft.Extensions.DependencyInjection;

namespace PizzaSaga.Shared.Infrastructure.DependencyInjection;

public static class MassTransitServiceCollectionExtensions
{
    /// <summary>
    /// Подключает MassTransit с RabbitMQ. Ожидает connection string "RabbitMQ" или "rabbitmq".
    /// </summary>
    public static IServiceCollection AddMassTransitWithRabbitMq(this IServiceCollection services, string rabbitMqConnectionString)
    {

        services.AddMassTransit(x =>
        {
            x.SetKebabCaseEndpointNameFormatter();

            x.UsingRabbitMq((context, cfg) =>
            {
                // Aspire уже возвращает URI-строку: amqp://guest:guest@host:port
                var uri = new Uri(rabbitMqConnectionString);
                cfg.Host(uri);

                // Автоматическая регистрация consumer'ов из сборки
                cfg.ConfigureEndpoints(context);
            });
        });

        return services;
    }
}