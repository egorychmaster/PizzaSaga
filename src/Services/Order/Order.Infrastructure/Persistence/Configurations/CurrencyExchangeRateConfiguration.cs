using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Order.Domain.AggregatesModel.Orders.ValueObjects;

namespace Order.Infrastructure.Persistence.Configurations;

/// <summary>
/// Конфигурация сущности CurrencyExchangeRate для EF Core.
/// Определяет маппинг на таблицу "CurrencyExchangeRates" с композитным PK.
/// </summary>
public sealed class CurrencyExchangeRateConfiguration : IEntityTypeConfiguration<CurrencyExchangeRate>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<CurrencyExchangeRate> builder)
    {
        // PK: (FromCurrencyCode, ToCurrencyCode)
        builder.HasKey(x => new { x.FromCurrencyCode, x.ToCurrencyCode });

        // Уникальный индекс
        builder.HasIndex(x => new { x.FromCurrencyCode, x.ToCurrencyCode }).IsUnique();

        // Маппинг на таблицу
        builder.ToTable("CurrencyExchangeRates");

        // FromCurrencyCode — всегда "USD" по заданию, но оставлено поле для гибкости
        builder.Property(x => x.FromCurrencyCode)
            .IsRequired()
            .HasMaxLength(3);

        builder.Property(x => x.ToCurrencyCode)
            .IsRequired()
            .HasMaxLength(3);

        // Rate: точность decimal(18,6) для корректного хранения курсов (например, 0.923456)
        builder.Property(x => x.Rate)
            .HasColumnType("decimal(18,6)");
    }
}
