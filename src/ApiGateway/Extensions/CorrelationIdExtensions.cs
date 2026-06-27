using System.Diagnostics;

namespace ApiGateway.Extensions;

/// <summary>
/// Extension для подключения корреляции (correlation id) через baggage и заголовок.
/// Гарантирует, что каждый входящий запрос получает уникальный ID, который пропагируется в трейсы и логи.
///
/// Приоритет:
///   1. CorrelationId из заголовка (от клиента или предыдущего сервиса)
///   2. CorrelationId из baggage (пропагируется через HttpClient/YARP)
///   3. Генерация нового ID (если ни один источник не найден).
/// </summary>
public static class CorrelationIdExtensions
{
    /// <summary>
    /// Подключает middleware, который:
    /// - генерирует correlation id при его отсутствии,
    /// - добавляет его в baggage (для исходящих вызовов),
    /// - помечает текущий Activity тегом и baggage,
    /// - логирует запрос для отладки.
    ///
    /// Обязателен для распределённой отладки: без него невозможно проследить цепочку
    /// Client → Gateway → OrderService → Stock → Payment в трейсах.
    /// </summary>
    public static IApplicationBuilder UseCorrelationId(this WebApplication app)
    {
        // Порядок подключения критичен — должен быть раньше всех остальных middleware
        // (особенно до UseAuthentication(), UseAuthorization() и YARP).
        app.Use(async (ctx, next) =>
        {
            string? correlationId = GetCorrelationId(ctx);

            if (string.IsNullOrEmpty(correlationId))
            {
                // Генерируем новый ID при первом входе
                correlationId = Guid.CreateVersion7().ToString();
                ctx.Request.Headers["CorrelationId"] = correlationId;
            }

            // Делаем доступным для OpenTelemetry через baggage — он пропагируется в HttpClient/YARP.
            Activity.Current?.AddBaggage("correlation.id", correlationId);

            // Добавляем как span-тег — так ID будет виден в Aspire Dashboard и Jaeger/Zipkin
            Activity.Current?.AddTag("correlation.id", correlationId);

            // 🔍 Отладочный лог для разработки (в продакшене можно убрать)
            Console.WriteLine($"[Gateway] {ctx.Request.Method} {ctx.Request.Path}, Corr={correlationId}");

            await next(ctx);
        });

        return app;
    }

    /// <summary>
    /// Извлекает correlation ID из baggage (приоритет) или заголовка.
    /// </summary>
    private static string? GetCorrelationId(HttpContext context)
    {
        // Приоритет 1: baggage — он пропагируется автоматически через HttpClient/YARP
        var baggage = Activity.Current?.Baggage.FirstOrDefault(x => x.Key == "correlation.id").Value;
        if (!string.IsNullOrEmpty(baggage))
            return baggage;

        // Приоритет 2: заголовок — если запрос пришёл напрямую (например, curl)
        if (context.Request.Headers.TryGetValue("CorrelationId", out var headerId))
            return headerId.ToString();

        return null;
    }
}