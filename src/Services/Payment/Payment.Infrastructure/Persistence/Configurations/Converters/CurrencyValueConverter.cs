using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PizzaSaga.SharedKernel.Domain.ValueObjects;

namespace Payment.Infrastructure.Persistence.Configurations.Converters;

/// <summary>
/// Преобразует доменный Value Object Currency в строковое представление для хранения в базе данных и обратно.
/// </summary>
internal sealed class CurrencyValueConverter : ValueConverter<Currency, string>
{
    public CurrencyValueConverter()
        : base(
            currency => currency.Code,
            code => Currency.Create(code))
    {
    }
}