using Microsoft.Extensions.DependencyInjection;

namespace PizzaSaga.Shared.ErrorHandling;

public static class GlobalProblemDetailsExceptionHandlerExtensions
{
    /// <summary>
    /// Регистрирует централизованную обработку исключений и формирование RFC 9457 ProblemDetails.
    /// </summary>
    public static IServiceCollection AddGlobalProblemDetailsExceptionHandling(
        this IServiceCollection services)
    {
        services.AddProblemDetails();
        services.AddExceptionHandler<GlobalProblemDetailsExceptionHandler>();

        return services;
    }
}