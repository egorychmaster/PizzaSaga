using PizzaSaga.SharedKernel.Domain.Exceptions;

namespace Order.Domain.AggregatesModel.Orders.Exceptions.Monies;

/// <summary>
/// Исключение, выбрасываемое при отсутствии курса конвертации для указанной пары валют.
/// </summary>
public sealed class MissingExchangeRateException : DomainException
{
    /// <summary>
    /// Создаёт экземпляр исключения с указанием пары валют.
    /// </summary>
    /// <param name="from">Код исходной валюты (например, "USD").</param>
    /// <param name="to">Код целевой валюты (например, "RUB").</param>
    public MissingExchangeRateException(string from, string to)
        : base($"No exchange rate found from {from} to {to}")
    {
    }
}
