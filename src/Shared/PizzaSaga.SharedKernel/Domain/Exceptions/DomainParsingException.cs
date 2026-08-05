namespace PizzaSaga.SharedKernel.Domain.Exceptions;

/// <summary>
/// Исключение, возникающее при невозможности преобразовать исходные данные в корректное значение доменной модели.
/// </summary>
public abstract class DomainParsingException : DomainException
{
    protected DomainParsingException(string message)
        : base(message)
    {
    }

    protected DomainParsingException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
