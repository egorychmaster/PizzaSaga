namespace Order.Application.Abstractions.Persistence.Idempotency;

/// <summary>
/// DTO запись идемпотентности.
/// Используется в Application слое для абстракции от EF Core сущностей.
/// </summary>
/// <param name="IdempotencyKey">Идентификатор идемпотентности (Guid из заголовка Idempotency-Key).</param>
/// <param name="RequestHash">SHA-256 хэш тела запроса.</param>
/// <param name="ResponseStatusCode">HTTP статус ответа при первоначальной обработке.</param>
/// <param name="ResponseBody">JSON-содержимое тела ответа.</param>
/// <param name="CreatedAt">Время создания записи в БД.</param>
public sealed record IdempotencyRecordDto(
    Guid IdempotencyKey,
    string RequestHash,
    int ResponseStatusCode,
    string ResponseBody,
    DateTimeOffset CreatedAt);