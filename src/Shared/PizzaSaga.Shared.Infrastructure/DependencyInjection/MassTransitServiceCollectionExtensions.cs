using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace PizzaSaga.Shared.Infrastructure.DependencyInjection;

/// <summary>
/// Расширения для подключения MassTransit с RabbitMQ.
/// </summary>
public static class MassTransitServiceCollectionExtensions
{
    /// <summary>
    /// Подключает MassTransit с RabbitMQ и автоматической регистрацией consumer'ов в отдельных очередях с префиксом.
    /// </summary>
    /// <param name="services">Коллекция сервисов.</param>
    /// <param name="rabbitMqConnectionString">Connection string RabbitMQ (формат amqp://...).</param>
    /// <param name="servicePrefix">Префикс для имён очередей.</param>
    /// <param name="consumerAssemblies">Сборки, содержащие реализации IConsumer.</param>
    public static IServiceCollection AddMassTransitWithRabbitMq(
        this IServiceCollection services, 
        string rabbitMqConnectionString,
        string servicePrefix,
        params Assembly[] consumerAssemblies)
    {
        services.AddMassTransit(x =>
        {
            // Регистрация потребителей из указанных сборок
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

    /// <summary>
    /// Подключает MassTransit с RabbitMQ и дополнительной конфигурацией bus (например, Saga).
    /// </summary>
    /// <param name="services">Коллекция сервисов.</param>
    /// <param name="rabbitMqConnectionString">Connection string RabbitMQ (формат amqp://...).</param>
    /// <param name="servicePrefix">Префикс для имён очередей.</param>
    /// <param name="configure">Доп. конфигурация bus (например, регистрация State Machine).</param>
    /// <param name="consumerAssemblies">Сборки с consumer'ами (необязательно).</param>
    public static IServiceCollection AddMassTransitWithRabbitMq(
    this IServiceCollection services,
    string rabbitMqConnectionString,
    string servicePrefix,
    Action<IBusRegistrationConfigurator> configure,
    Assembly[] consumerAssemblies = null
        )
    {
        ArgumentNullException.ThrowIfNull(configure);

        services.AddMassTransit(x =>
        {
            // Регистрация потребителей из указанных сборок
            if (consumerAssemblies is { Length: > 0 })
                x.AddConsumers(consumerAssemblies);

            // Вызов дополнительной конфигурации (например, регистрация saga)
            configure(x);

            x.UsingRabbitMq((context, cfg) =>
            {
                var uri = new Uri(rabbitMqConnectionString);
                cfg.Host(uri);

                // Автоматическая настройка endpoints с учётом зарегистрированных компонентов
                cfg.ConfigureEndpoints(context);
            });
        });

        return services;
    }
}
