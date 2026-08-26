namespace Order.Application.Abstractions.Persistence.Idempotency.Exceptions;

/// <summary>
/// Исключение, возникающее при попытке зарегистрировать уже существующий Idempotency-Key.
/// </summary>
public sealed class DuplicateIdempotencyKeyException : Exception
{
    /// <summary>
    /// Инициализирует исключение о конфликте Idempotency-Key.
    /// </summary>
    public DuplicateIdempotencyKeyException()
        : base("The specified Idempotency-Key is already being processed or has already been processed.")
    {
    }

    /// <summary>
    /// Инициализирует исключение о конфликте Idempotency-Key с исходным исключением.
    /// </summary>
    public DuplicateIdempotencyKeyException(Exception innerException)
        : base(
            "The specified Idempotency-Key is already being processed or has already been processed.",
            innerException)
    {
    }
}