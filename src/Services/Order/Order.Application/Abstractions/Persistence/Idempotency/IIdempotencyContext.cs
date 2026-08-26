namespace Order.Application.Abstractions.Persistence.Idempotency;

/// <summary>
/// Контекст идемпотентности текущего HTTP-запроса.
/// Используется для передачи параметров между API и application pipeline.
/// </summary>
public interface IIdempotencyContext
{
    /// <summary>
    /// Идемпотентный ключ, полученный из заголовка Idempotency-Key HTTP-запроса.
    /// </summary>
    Guid Key { get; }

    /// <summary>
    /// SHA-256 hash тела запроса.
    /// </summary>
    string? RequestHash { get; }

    /// <summary>
    /// HTTP status code успешного ответа, который должен быть сохранён.
    /// </summary>
    int ResponseStatusCode { get; }

    /// <summary>
    /// Возвращает true, если ключ и hash запроса установлены.
    /// </summary>
    bool IsSet { get; }

    /// <summary>
    /// Устанавливает параметры идемпотентности запроса.
    /// </summary>
    /// <param name="key">Идемпотентный ключ (Guid), полученный из HTTP-заголовка Idempotency-Key.</param>
    /// <param name="requestHash">SHA-256 хэш тела HTTP-запроса в виде строки.</param>
    void Set(Guid key, string requestHash);

    /// <summary>
    /// Устанавливает HTTP status code сохраняемого ответа.
    /// </summary>
    void SetResponseStatusCode(int statusCode);
}

/// <summary>
/// Scoped-контекст параметров идемпотентности текущего HTTP-запроса.
/// </summary>
public sealed class IdempotencyContext : IIdempotencyContext
{
    private Guid _key;

    /// <inheritdoc />
    public Guid Key
    {
        get => _key;
        private set => _key = value != Guid.Empty
            ? value
            : throw new InvalidOperationException("Key cannot be set to Guid.Empty.");
    }

    /// <inheritdoc />
    public string? RequestHash { get; private set; }

    /// <inheritdoc />
    public int ResponseStatusCode { get; private set; }

    /// <inheritdoc />
    public bool IsSet =>
        Key != Guid.Empty &&
        !string.IsNullOrEmpty(RequestHash);

    /// <inheritdoc />
    public void Set(Guid key, string requestHash)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(requestHash);

        Key = key;
        RequestHash = requestHash;
    }

    /// <inheritdoc />
    public void SetResponseStatusCode(int statusCode)
    {
        if (statusCode is < 100 or > 599)
        {
            throw new ArgumentOutOfRangeException(
                nameof(statusCode),
                statusCode,
                "HTTP status code must be between 100 and 599.");
        }

        ResponseStatusCode = statusCode;
    }
}