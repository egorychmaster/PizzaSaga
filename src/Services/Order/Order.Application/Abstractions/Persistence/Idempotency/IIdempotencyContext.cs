namespace Order.Application.Abstractions.Persistence.Idempotency;

/// <summary>
/// Интерфейс Scoped-контекста для передачи параметров идемпотентности из HTTP-слоя в пайплайн обработки (Pipeline).
/// </summary>
public interface IIdempotencyContext
{
    /// <summary>
    /// Идемпотентный ключ, полученный из заголовка Idempotency-Key HTTP-запроса.
    /// </summary>
    Guid Key { get; }

    /// <summary>
    /// SHA-256 хэш тела HTTP-запроса. 
    /// Null или пустая строка, если хэш ещё не вычислен.
    /// </summary>
    string? RequestHash { get; }

    /// <summary>
    /// Возвращает true, если оба поля (Key и RequestHash) успешно установлены.
    /// Используется для проверки готовности контекста к использованию в транзакционной логике.
    /// </summary>
    bool IsSet { get; }

    /// <summary>
    /// Устанавливает параметры идемпотентности.
    /// Вызывается из IdempotencyFilter после парсинга заголовка и вычисления хэша тела запроса.
    /// </summary>
    /// <param name="key">Идемпотентный ключ (Guid), полученный из HTTP-заголовка Idempotency-Key.</param>
    /// <param name="requestHash">SHA-256 хэш тела HTTP-запроса в виде строки.</param>
    void Set(Guid key, string requestHash);
}

/// <summary>
/// Реализация IIdempotencyContext для передачи параметров идемпотентности из HTTP-слоя в Application/Infrastructure.
/// </summary>
public sealed class IdempotencyContext : IIdempotencyContext
{
    private Guid _key;

    /// <inheritdoc />
    public Guid Key
    {
        get => _key;
        private set => _key = value != Guid.Empty ? value : throw new InvalidOperationException("Key cannot be set to Guid.Empty.");
    }

    /// <inheritdoc />
    public string? RequestHash { get; private set; }

    /// <inheritdoc />
    public bool IsSet => Key != Guid.Empty && !string.IsNullOrEmpty(RequestHash);

    /// <inheritdoc />
    public void Set(Guid key, string requestHash)
    {
        Key = key;
        RequestHash = requestHash;
    }
}