using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Payment.Infrastructure.MassTransit.CommandConsumers;
using Payment.Infrastructure.Persistence;
using PizzaSaga.Shared.Infrastructure.DependencyInjection;

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


        // Подключаем MassTransit с RabbitMQ и указываем сборку consumer'ов
        services.AddMassTransitWithRabbitMq(rabbitMqConnectionString, "Payment", typeof(AuthorizePaymentConsumer).Assembly);

        return services;
    }
}
