using Microsoft.EntityFrameworkCore;
using Order.Domain.AggregatesModel.Orders.ValueObjects;
using PizzaSaga.Shared.Infrastructure.Persistence;

namespace Order.Infrastructure.Persistence.Seeding;

public sealed class OrderDatabaseSeeder : IDatabaseSeeder<OrderDbContext>
{
    public async Task SeedAsync(OrderDbContext context, CancellationToken cancellationToken)
    {
        if (await context.CurrencyExchangeRates.AnyAsync(cancellationToken))
            return;

        // Заполняем таблицу курсов валют.
        // Так как из микросервиса каталога приходит цена товара всегда в USD, то конвертация в другие валюты быдет всегда из USD.
        var rates = new[]
        {
            new CurrencyExchangeRate { FromCurrencyCode = "USD", ToCurrencyCode = "RUB", Rate = 108.5m },
            new CurrencyExchangeRate { FromCurrencyCode = "USD", ToCurrencyCode = "EUR", Rate = 0.92m }
        };

        await context.CurrencyExchangeRates.AddRangeAsync(rates, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }
}