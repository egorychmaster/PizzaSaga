using Microsoft.AspNetCore.Builder;
using Serilog;

namespace PizzaSaga.ServiceDefaults.Extensions;

public static class SerilogExtensions
{
    /// <summary>
    /// Подключает Serilog как центральный провайдер логирования, конфигурируя его через appsettings.json, DI-сервисы и контекст лога. 
    /// Логи автоматически экспортируются в OpenTelemetry (LogRecord’ы) с baggage (например, correlation.id), 
    /// что позволяет связывать логи с трейсами в Aspire Dashboard.
    /// Вызывается один раз в каждом сервисе через AddServiceDefaults().
    /// </summary>
    /// <param name="builder">Конвейер построения приложения .NET.</param>
    /// <returns>Тот же экземпляр <see cref="WebApplicationBuilder"/> для цепочки вызовов.</returns>
    public static WebApplicationBuilder AddSerilogDefaults(this WebApplicationBuilder builder)
    {
        builder.Host.UseSerilog((ctx, services, lc) =>
        {
            lc.ReadFrom.Configuration(ctx.Configuration)
              .ReadFrom.Services(services)
              .Enrich.FromLogContext();
        });

        return builder;
    }
}