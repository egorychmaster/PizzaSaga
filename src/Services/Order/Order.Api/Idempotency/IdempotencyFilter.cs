using Order.Application.Abstractions.Persistence.Idempotency;

namespace Order.Api.Idempotency;

/// <summary>
/// Endpoint-фильтр для обеспечения идемпотентности POST /api/v1/orders.
/// Выполняет Fast Path, вычисляет хеш и устанавливает IIdempotencyContext.
/// Обработка race-условий делегируется IdempotencyBehavior → TransactionBehavior → UnitOfWork.
/// </summary>
public sealed class IdempotencyFilter : IEndpointFilter
{
    private const string HeaderName = "Idempotency-Key";

    private readonly IIdempotencyRepository _idempotencyRepo;
    private readonly IIdempotencyContext _idempotencyContext;
    private readonly ILogger<IdempotencyFilter> _logger;

    public IdempotencyFilter(
        IIdempotencyRepository idempotencyRepo,
        IIdempotencyContext idempotencyContext,
        ILogger<IdempotencyFilter> logger)
    {
        _idempotencyRepo = idempotencyRepo ?? throw new ArgumentNullException(nameof(idempotencyRepo));
        _idempotencyContext = idempotencyContext ?? throw new ArgumentNullException(nameof(idempotencyContext));
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

        // Передаём управление дальше по pipeline (Endpoint → Mediator → TransactionBehavior)
        return await next(context);
    }

    private IResult HandleExistingRecord(IdempotencyRecordDto record, string currentRequestHash, HttpContext context)
    {
        if (!string.Equals(record.RequestHash, currentRequestHash, StringComparison.OrdinalIgnoreCase))
        {
            return Results.Problem(
                title: "Idempotency-Key conflict.",
                detail: "This Idempotency-Key has already been used with a different request payload.",
                statusCode: StatusCodes.Status409Conflict,
                instance: context.Request.Path.ToString(),
                type: "urn:pizzasaga:error:idempotency-key-duplicate-with-different-body");
        }

        return Results.Content(
            content: record.ResponseBody,
            contentType: "application/json",
            statusCode: record.ResponseStatusCode);
    }
}