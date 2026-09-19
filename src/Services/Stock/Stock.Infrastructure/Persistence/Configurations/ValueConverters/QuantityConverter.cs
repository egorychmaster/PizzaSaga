using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Stock.Domain.AggregatesModel.Inventory.ValueObjects;

namespace Stock.Infrastructure.Persistence.Configurations.ValueConverters;

/// <summary>
/// Преобразование между Quantity и int для EF Core.
/// </summary>
public static class QuantityConverter
{
    /// <summary>
    /// Создаёт ValueConverter для Quantity.
    /// </summary>
    public static ValueConverter<Quantity, int> Create()
        => new(
            v => v.Value,
            v => Quantity.Create(v));
}
