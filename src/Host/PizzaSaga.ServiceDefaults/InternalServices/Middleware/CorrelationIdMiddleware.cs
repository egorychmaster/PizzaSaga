using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace PizzaSaga.ServiceDefaults.InternalServices.Middleware;

// Extension для удобного подключения
public static class CorrelationIdMiddlewareExtensions
{
    /// <summary>
    /// Пропагирует уже установленный CorrelationId: берёт из baggage или заголовка и добавляет в span-теги + логи.
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<CorrelationIdMiddleware>();
    }
}

/// <summary>
/// Выполняется на каждый входящий HTTP-запрос.
/// </summary>
public class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<CorrelationIdMiddleware> _logger;

    public CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    /// <summary>
    /// Выполняется на каждый входящий HTTP-запрос
    /// Алгоритм:
    /// 1. Ищет correlationid в Activity.Current.Baggage (от YARP/HttpClient)
    /// 2. Если нет — проверяет заголовок CorrelationId
    /// 3. Если нашёл → добавляет его в:
    ///     - baggage(для исходящих запросов к другим сервисам)
    ///     - span-теги(correlationid) — чтобы видеть в Aspire Dashboard
    ///     - log scope(через LogContext.PushProperty) — для Serilog/логирования
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = GetCorrelationId(context);

        if (!string.IsNullOrEmpty(correlationId))
        {
            var traceId = Activity.Current?.TraceId.ToString() ?? "N/A";

            // Сохраняем для логов
            using (Serilog.Context.LogContext.PushProperty("CorrelationId", correlationId))
            using (Serilog.Context.LogContext.PushProperty("TraceId", traceId))
            {

                // Прописываем в baggage (для OpenTelemetry)
                Activity.Current?.AddBaggage("correlationid", correlationId);

                // Добавляем как span-тег — так он будет в Aspire Dashboard
                Activity.Current?.AddTag("correlationid", correlationId);

                using (_logger.BeginScope(new Dictionary<string, object?> { ["CorrelationId"] = correlationId }))
                {
                    _logger.LogInformation("CorrelationId middleware (correlationId={correlationId}, traceId={traceId}).", correlationId, traceId);
                    await _next(context);
                }
            }
        }
        else
        {
            await _next(context); // Корреляции нет — возможно, запрос без gateway (тестовый)
        }
    }

    /// <summary>
    /// Приоритет источника: baggage > заголовок
    /// Это важно: baggage пропагируется через HttpClient/YARP автоматически, а заголовок может быть установлен клиентом или предыдущим сервисом.
    /// </summary>
    private static string? GetCorrelationId(HttpContext context)
    {
        // Приоритет 1: baggage
        var baggage = Activity.Current?.Baggage.FirstOrDefault(x => x.Key == "correlationid").Value;
        if (!string.IsNullOrEmpty(baggage))
            return baggage;

        // Приоритет 2: заголовок
        if (context.Request.Headers.TryGetValue("CorrelationId", out var cidHeader))
            return cidHeader.ToString();

        return null;
    }
}

