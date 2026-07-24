using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using PizzaSaga.ServiceDefaults.ErrorHandling;

namespace PizzaSaga.ServiceDefaults.Extensions;

public static class GlobalExceptionHandlerExtensions
{
    public static IServiceCollection AddGlobalExceptionHandling(this IServiceCollection services)
    {
        services.AddProblemDetails(); // RFC 9457
        services.AddExceptionHandler<GlobalExceptionHandler>();
        return services;
    }

    // Для app builder — добавляем middleware
    public static IApplicationBuilder UseGlobalExceptionHandling(this IApplicationBuilder app)
    {
        app.UseExceptionHandler(options => options.Run(async context =>
        {
            var exceptionHandler = context.RequestServices.GetRequiredService<IExceptionHandler>();
            var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;
            if (exception != null)
            {
                await exceptionHandler.TryHandleAsync(context, exception, CancellationToken.None);
            }
        }));
        return app;
    }
}
