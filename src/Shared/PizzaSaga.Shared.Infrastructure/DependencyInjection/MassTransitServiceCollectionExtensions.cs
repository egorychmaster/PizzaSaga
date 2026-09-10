using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace PizzaSaga.Shared.Infrastructure.DependencyInjection;

public static class MassTransitServiceCollectionExtensions
{
    /// <summary>
    /// Подключает MassTransit с RabbitMQ. Ожидает connection string "RabbitMQ" или "rabbitmq".
    /// </summary>
    /// <param name="services"></param>
    /// <param name="rabbitMqConnectionString"></param>
    /// <param name="consumerAssemblies">Сборки в котрых надо регистрировать потребителей.</param>
    /// <returns></returns>
    public static IServiceCollection AddMassTransitWithRabbitMq(this IServiceCollection services, string rabbitMqConnectionString, params Assembly[] consumerAssemblies)
    {

        services.AddMassTransit(x =>
        {
            x.SetKebabCaseEndpointNameFormatter();

            // Регистрируем всех потребителей из переданных сборок
            if (consumerAssemblies is { Length: > 0 })
            {
                x.AddConsumers(consumerAssemblies);
            }

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