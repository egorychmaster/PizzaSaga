using Microsoft.AspNetCore.Builder;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using System.Diagnostics;

namespace PizzaSaga.ServiceDefaults.Extensions;

public static class SerilogExtensions
{
    /// <summary>
    /// Подключает Serilog как центральный провайдер логирования, конфигурируя его через appsettings.json, DI-сервисы и контекст лога. 
    /// Логи автоматически экспортируются в OpenTelemetry (LogRecord’ы) с baggage (например, correlationid), 
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
              .Enrich.FromLogContext()
              // Динамическое добавление TraceId к каждому событию логирования
              .Enrich.With(new TraceIdEnricher());
        });

        return builder;
    }
}

public sealed class TraceIdEnricher : ILogEventEnricher
{
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var traceId = Activity.Current?.TraceId.ToString();
        if (!string.IsNullOrEmpty(traceId))
        {
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("TraceId", traceId));
        }
    }
}