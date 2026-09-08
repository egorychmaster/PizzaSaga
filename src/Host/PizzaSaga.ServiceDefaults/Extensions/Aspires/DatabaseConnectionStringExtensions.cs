using Microsoft.Extensions.Configuration;

namespace PizzaSaga.ServiceDefaults.Extensions.Aspires;

public static class DatabaseConnectionStringExtensions
{
    /// <summary>
    /// Получает connection string к PostgreSQL из конфигурации.
    /// Бросает исключение, если строка отсутствует.
    /// </summary>
    public static string GetDatabaseConnectionString(this IConfiguration configuration, string name)
    {
        var connectionString = configuration.GetConnectionString(name)
                            ?? throw new InvalidOperationException(
                                $"Connection string '{name}' of the database is not configured.");

        return connectionString;
    }
}