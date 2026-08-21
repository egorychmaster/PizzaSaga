namespace Order.Infrastructure.Persistence.Idempotency;

/// <summary>
/// Сущность для хранения данных идемпотентности HTTP-запроса.
/// Используется для предотвращения повторной обработки одного и того же запроса.
/// </summary>
public sealed class IdempotencyRecord
{
    /// <summary>
    /// Уникальный ключ, который клиент передает в заголовке Idempotency-Key.
    /// Это PRIMARY KEY таблицы.
    /// </summary>
    public Guid IdempotencyKey { get; set; }

    /// <summary>
    /// SHA-256 хэш тела HTTP-запроса. Используется для проверки идентичности запросов.
    /// </summary>
    public string RequestHash { get; set; } = default!;

    /// <summary>
    /// Текущее состояние обработки запроса.
    /// </summary>
    //public IdempotencyRecordStatus Status { get; set; }

    /// <summary>
    /// HTTP статус-код, который был возвращен при первоначальной обработке.
    /// Например: 201 (Created).
    /// </summary>
    public int ResponseStatusCode { get; set; }

    /// <summary>
    /// JSON-содержимое тела ответа при первоначальной обработке.
    /// Хранится как PostgreSQL jsonb для возможного анализа или повторной десериализации.
    /// </summary>
    public required string ResponseBody { get; set; }

    /// <summary>
    /// Время создания записи в таблице идемпотентности.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }
}