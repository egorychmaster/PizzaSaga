using PizzaSaga.SharedKernel.Domain.Exceptions;

namespace Stock.Domain.AggregatesModel.InventoryReservation.Exceptions;

/// <summary>
/// Исключение, возникающее при попытке выполнить недопустимую операцию над резервом.
/// </summary>
public sealed class InvalidReservationStateException : DomainException
{
    /// <summary>
    /// Инициализирует новый экземпляр исключения.
    /// </summary>
    public InvalidReservationStateException(string message)
        : base(message)
    {
    }
}
