using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
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

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    /// <summary>
    /// Выполняется на каждый входящий HTTP-запрос
    /// Алгоритм:
    /// 1. Ищет correlation.id в Activity.Current.Baggage (от YARP/HttpClient)
    /// 2. Если нет — проверяет заголовок CorrelationId
    /// 3. Если нашёл → добавляет его в:
    ///     - baggage(для исходящих запросов к другим сервисам)
    ///     - span-теги(correlation.id) — чтобы видеть в Aspire Dashboard
    ///     - log scope(через LogContext.PushProperty) — для Serilog/логирования
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public async Task InvokeAsync(HttpContext context)
    {
        var cid = GetCorrelationId(context);

        if (!string.IsNullOrEmpty(cid))
        {
            // Прописываем в baggage (для OpenTelemetry)
            Activity.Current?.AddBaggage("correlation.id", cid);

            // Добавляем как span-тег — так он будет в Aspire Dashboard
            Activity.Current?.AddTag("correlation.id", cid);

            // Сохраняем для логов — если используешь Serilog/LogContext
            //using (var _ = Microsoft.Extensions.Logging.LogContext.PushProperty("CorrelationId", cid))
            //{
                await _next(context);
            //}
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
        var baggage = Activity.Current?.Baggage.FirstOrDefault(x => x.Key == "correlation.id").Value;
        if (!string.IsNullOrEmpty(baggage))
            return baggage;

        // Приоритет 2: заголовок
        if (context.Request.Headers.TryGetValue("CorrelationId", out var cidHeader))
            return cidHeader.ToString();

        return null;
    }
}

