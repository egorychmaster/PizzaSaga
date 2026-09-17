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
    /// <param name="servicePrefix">Префикс сервиса для добавления в начало имени очереди.</param>
    /// <param name="consumerAssemblies">Сборки в котрых надо регистрировать потребителей.</param>
    /// <returns></returns>
    public static IServiceCollection AddMassTransitWithRabbitMq(
        this IServiceCollection services, 
        string rabbitMqConnectionString,
        string servicePrefix,
        params Assembly[] consumerAssemblies)
    {
        services.AddMassTransit(x =>
        {
            if (consumerAssemblies is { Length: > 0 })
            {
                x.AddConsumers(consumerAssemblies);
            }

            x.UsingRabbitMq((context, cfg) =>
            {
                // Aspire уже возвращает URI-строку: amqp://guest:guest@host:port
                var uri = new Uri(rabbitMqConnectionString);
                cfg.Host(uri);

                // Явно задаём уникальное имя очереди с префиксом имени сервиса для каждого consumer'а,
                // чтобы избежать конфликта при дублировании типов потребителей в разных сервисах.
                foreach (var consumerType in consumerAssemblies
                    .SelectMany(a => a.GetTypes())
                    .Where(t => typeof(IConsumer).IsAssignableFrom(t)))
                {
                    var queueName = $"{servicePrefix}-{consumerType.Name}";
                    cfg.ReceiveEndpoint(queueName, e =>
                    {
                        e.ConfigureConsumer(context, consumerType);
                    });
                }
            });
        });

        return services;
    }
}
