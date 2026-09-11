using Microsoft.EntityFrameworkCore;
using Order.Domain.Abstractions.Repositories;
using Order.Domain.AggregatesModel.Orders.Exceptions.Monies;

namespace Order.Infrastructure.Persistence.Repositories;

/// <summary>
/// Реализация репозитория для получения курсов конвертации валют.
/// Используется при создании заказа для конвертации цены из USD в валюту пользователя.
/// </summary>
public sealed class CurrencyExchangeRateRepository : ICurrencyExchangeRateRepository
{
    private readonly OrderDbContext _context;

    /// <summary>
    /// Создаёт экземпляр репозитория.
    /// </summary>
    /// <param name="context">Контекст базы данных.</param>
    public CurrencyExchangeRateRepository(OrderDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <inheritdoc />
    public async Task<decimal> GetRateAsync(string from, string to, CancellationToken cancellationToken)
    {
        var rate = await _context.CurrencyExchangeRates
            .AsNoTracking()
            .Where(x => x.FromCurrencyCode == from && x.ToCurrencyCode == to)
            .Select(x => x.Rate)
            .FirstOrDefaultAsync(cancellationToken);

        if (rate == 0m) // не найдено
            throw new MissingExchangeRateException(from, to);

        return rate;
    }
}
