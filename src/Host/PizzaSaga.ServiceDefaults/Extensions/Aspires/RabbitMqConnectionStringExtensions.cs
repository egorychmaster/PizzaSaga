using Microsoft.Extensions.Configuration;

namespace PizzaSaga.ServiceDefaults.Extensions.Aspires;

public static class RabbitMqConnectionStringExtensions
{
    /// <summary>
    /// Получает connection string к RabbitMQ из конфигурации.
    /// Поддерживает как "rabbitmq" (Aspire), так и "RabbitMQ".
    /// Бросает исключение, если строка отсутствует.
    /// </summary>
    public static string GetRabbitMqConnectionString(this IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("rabbitmq")
                            ?? configuration["RabbitMQ"]
                            ?? throw new InvalidOperationException(
                                "Connection string 'rabbitmq' or key 'RabbitMQ' is not configured.");

        return connectionString;
    }
}