namespace Order.Domain.Abstractions.Repositories;

/// <summary>
/// Репозиторий для получения курсов конвертации валют.
/// </summary>
public interface ICurrencyExchangeRateRepository
{
    /// <summary>
    /// Получает курс конвертации из `from` в `to`. Бросает исключение, если не найден.
    /// </summary>
    /// <param name="from">Код исходной валюты (обычно "USD").</param>
    /// <param name="to">Код целевой валюты (например, "RUB", "EUR").</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Курс конвертации (количество единиц `to` за 1 единицу `from`).</returns>
    /// <exception cref="MissingExchangeRateException">Если курс для указанной пары валют не найден.</exception>
    Task<decimal> GetRateAsync(string from, string to, CancellationToken cancellationToken);
}
