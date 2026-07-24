using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace PizzaSaga.ServiceDefaults.ErrorHandling;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) =>
        _logger = logger;

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        // Логируем с correlation.id и traceId (они уже в Serilog scope через CorrelationIdMiddleware + Activity.Current)
        _logger.LogError(exception, "Unhandled exception occurred while processing request {Path}", httpContext.Request.Path);

        // Возвращаем ProblemDetails без деталей
        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "An error occurred while processing your request.",
            Instance = httpContext.Request.Path,
        };

        // Добавляем публичный traceId (из Activity)
        if (httpContext.TraceIdentifier is { } traceId)
            problemDetails.Extensions["traceId"] = traceId;

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
        httpContext.Response.ContentType = "application/problem+json";

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true; // исключение обработано
    }
}
