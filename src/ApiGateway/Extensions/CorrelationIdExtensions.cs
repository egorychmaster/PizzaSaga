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
///   
/// Порядок подключения критичен — должен быть раньше всех остальных middleware.
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
    public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder builder)
        => builder.UseMiddleware<CorrelationIdMiddleware>();

    // Внутренний middleware-класс
    private sealed class CorrelationIdMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<CorrelationIdMiddleware> _logger;

        public CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var correlationId = GetCorrelationId(context);

            if (string.IsNullOrEmpty(correlationId))
            {
                // Генерируем новый ID при первом входе
                correlationId = Guid.CreateVersion7().ToString();
                context.Request.Headers["CorrelationId"] = correlationId;
            }

            // Прописываем в baggage (для исходящих запросов)
            Activity.Current?.AddBaggage("correlation.id", correlationId);

            // Добавляем как span-тег — так ID будет виден в Aspire Dashboard
            Activity.Current?.AddTag("correlation.id", correlationId);

            // 🟢 Продакшен-дружелюбное логирование
            _logger.LogInformation("[Gateway] {Method} {Path} (correlationId={correlationId})",
                context.Request.Method, context.Request.Path, correlationId);

            await _next(context);
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
}