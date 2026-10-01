namespace PizzaSaga.SharedKernel.Domain.Exceptions;

/// <summary>
/// Базовое исключение для сценариев, когда запрашиваемый ресурс не найден.
/// </summary>
public abstract class NotFoundException : DomainException
{
    /// <summary>
    /// Создаёт экземпляр исключения.
    /// </summary>
    /// <param name="message">Сообщение об ошибке.</param>
    protected NotFoundException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Создаёт экземпляр исключения с внутренним исключением.
    /// </summary>
    /// <param name="message">Сообщение об ошибке.</param>
    /// <param name="innerException">Внутреннее исключение.</param>
    protected NotFoundException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
