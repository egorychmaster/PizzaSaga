using Microsoft.Extensions.DependencyInjection;

namespace Catalog.Application.DependencyInjection;

/// <summary>
/// Расширения для регистрации зависимостей слоя Application.
/// </summary>
public static class ApplicationServiceCollectionExtensions
{
    /// <summary>
    /// Регистрирует зависимости слоя Application.
    /// </summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Регистрируем Mediator.
        services.AddMediator(options =>
        {
            // ServiceLifetime = Scoped позволяет использовать scoped сервисы
            options.ServiceLifetime = ServiceLifetime.Scoped;
        });

        return services;
    }
}
