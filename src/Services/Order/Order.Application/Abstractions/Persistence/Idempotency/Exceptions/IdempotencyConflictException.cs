namespace Order.Application.Abstractions.Persistence.Idempotency.Exceptions;

/// <summary>
/// Исключение, возникающее при конфликте идемпотентности.
/// Например: один Idempotency-Key использован с разными телами запросов.
/// </summary>
public sealed class IdempotencyConflictException : Exception
{
    /// <summary>
    /// Инициализирует исключение о конфликте идемпотентности.
    /// </summary>
    public IdempotencyConflictException(string message)
        : base(message)
    {
    }
}