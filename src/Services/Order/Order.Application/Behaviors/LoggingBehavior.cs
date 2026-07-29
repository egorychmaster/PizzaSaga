using Mediator;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Order.Application.Behaviors;

/// <summary>
/// Pipeline behavior для структурированного логирования выполнения сообщений Mediator.
/// Логирует начало обработки, успешное завершение с длительностью и исключения.
/// </summary>
/// <typeparam name="TMessage">Тип сообщения Mediator.</typeparam>
/// <typeparam name="TResponse">Тип ответа обработчика.</typeparam>
public sealed class LoggingBehavior<TMessage, TResponse>
    : IPipelineBehavior<TMessage, TResponse>
    where TMessage : IMessage
{
    private readonly ILogger<LoggingBehavior<TMessage, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TMessage, TResponse>> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Выполняет логирование и передаёт управление следующему этапу pipeline.
    /// </summary>
    public async ValueTask<TResponse> Handle(TMessage message, MessageHandlerDelegate<TMessage, TResponse> next, CancellationToken cancellationToken)
    {
        var messageName = typeof(TMessage).Name;

        // Stopwatch предназначен именно для измерения elapsed time.
        // В отличие от DateTimeOffset, он не зависит от изменения системных часов.
        var startedAt = Stopwatch.GetTimestamp();

        _logger.LogInformation("Handling {MessageName}", messageName);

        try
        {
            // Передаём выполнение следующему Behavior или непосредственно Handler.
            var response = await next(message, cancellationToken);

            var elapsed = Stopwatch.GetElapsedTime(startedAt);

            _logger.LogInformation("Handled {MessageName} in {ElapsedMilliseconds} ms", messageName, elapsed.TotalMilliseconds);

            return response;
        }
        catch (Exception ex)
        {
            var elapsed = Stopwatch.GetElapsedTime(startedAt);

            _logger.LogError(ex, "Error handling {MessageName} after {ElapsedMilliseconds} ms", messageName, elapsed.TotalMilliseconds);

            // Не поглощаем исключение.
            // Оно должно продолжить движение вверх по pipeline.
            throw;
        }
    }
}