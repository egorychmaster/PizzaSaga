using Microsoft.EntityFrameworkCore;
using Order.Application.Abstractions.Persistence.Idempotency;

namespace Order.Api.Idempotency;

/// <summary>
/// Endpoint-фильтр для обеспечения идемпотентности POST /api/v1/orders.
/// </summary>
public sealed class IdempotencyFilter : IEndpointFilter
{
    private const string HeaderName = "Idempotency-Key";

    private readonly IIdempotencyRepository _idempotencyRepo;
    private readonly IIdempotencyContext _idempotencyContext;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<IdempotencyFilter> _logger;

    public IdempotencyFilter(
        IIdempotencyRepository idempotencyRepo,
        IIdempotencyContext idempotencyContext,
        IServiceProvider serviceProvider,
        ILogger<IdempotencyFilter> logger)
    {
        _idempotencyRepo = idempotencyRepo ?? throw new ArgumentNullException(nameof(idempotencyRepo));
        _idempotencyContext = idempotencyContext ?? throw new ArgumentNullException(nameof(idempotencyContext));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var httpContext = context.HttpContext;

        // Шаг 1: Обязательная проверка заголовка Idempotency-Key
        if (!httpContext.Request.Headers.TryGetValue(HeaderName, out var headerValue) ||
            !Guid.TryParse(headerValue, out var idempotencyKey) ||
            idempotencyKey == Guid.Empty)
        {
            return Results.Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Idempotency-Key header is required.",
                detail: $"Header '{HeaderName}' is missing or contains an invalid Guid format.",
                instance: httpContext.Request.Path.ToString());
        }

        // Шаг 2: Вычисляем SHA-256 хеш от body
        var requestHash = RequestHashCalculator.ComputeHash(httpContext.Request);

        // Шаг 3: Fast Path — проверяем наличие записи в БД
        var existingRecord = await _idempotencyRepo.GetAsync(idempotencyKey, httpContext.RequestAborted);
        if (existingRecord is not null)
        {
            return HandleExistingRecord(existingRecord, requestHash, httpContext);
        }

        // Шаг 4: Передаем ключ и хеш в Scoped Context для сохранения ВНУТРИ единой транзакции
        _idempotencyContext.Set(idempotencyKey, requestHash);

        try
        {
            // Передаем управление дальше по pipeline (Endpoint -> Mediator -> TransactionBehavior)
            return await next(context);
        }
        catch (DbUpdateException ex) when (IsDuplicateIdempotencyKey(ex))
        {
            // Обработка Race Condition (Параллельные запросы с одинаковым Idempotency-Key)
            _logger.LogWarning("Concurrent request for Idempotency-Key {Key} caused unique constraint violation. Handling fallback...", idempotencyKey);

            // Так как текущая транзакция и DbContext находятся в сбойном состоянии после Rollback, поднимаем изолированный Scope для повторного чтения из БД.
            using var scope = _serviceProvider.CreateScope();
            var isolatedRepo = scope.ServiceProvider.GetRequiredService<IIdempotencyRepository>();

            var retryRecord = await isolatedRepo.GetAsync(idempotencyKey, httpContext.RequestAborted);
            if (retryRecord is null)
            {
                return Results.Problem(
                    title: "Concurrent request is being processed.",
                    detail: "A concurrent request with the same Idempotency-Key is currently being processed. Please retry later.",
                    statusCode: StatusCodes.Status409Conflict,
                    instance: httpContext.Request.Path.ToString(),
                    type: "urn:pizzasaga:error:idempotency-key-concurrent");
            }

            return HandleExistingRecord(retryRecord, requestHash, httpContext);
        }
    }

    private IResult HandleExistingRecord(IdempotencyRecordDto record, string currentRequestHash, HttpContext context)
    {
        if (record.RequestHash == currentRequestHash)
        {
            return Results.Content(
                content: record.ResponseBody,
                contentType: "application/json",
                statusCode: record.ResponseStatusCode);
        }

        return Results.Problem(
            title: "Idempotency-Key conflict.",
            detail: "This Idempotency-Key has already been used with a different request payload.",
            statusCode: StatusCodes.Status409Conflict,
            instance: context.Request.Path.ToString(),
            type: "urn:pizzasaga:error:idempotency-key-duplicate-with-different-body");
    }

    private static bool IsDuplicateIdempotencyKey(DbUpdateException ex) =>
        ex.InnerException?.Message.Contains("23505") == true || // PostgreSQL unique_violation
        ex.Message.Contains("unique constraint", StringComparison.OrdinalIgnoreCase);
}
