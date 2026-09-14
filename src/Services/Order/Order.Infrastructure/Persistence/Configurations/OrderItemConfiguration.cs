using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Order.Domain.AggregatesModel.Orders;
using Order.Infrastructure.Persistence.Configurations.Converters;

namespace Order.Infrastructure.Persistence.Configurations;

/// <summary>
/// Конфигурация сущности для Entity Framework Core.
/// </summary>
internal sealed class OrderItemConfiguration
    : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        // Primary key
        builder.HasKey(x => x.Id);

        // Columns
        builder.Property(x => x.ProductId)
            .IsRequired();

        // Quantity
        builder.OwnsOne(
            x => x.Quantity,
            quantity =>
            {
                quantity.Property(x => x.Value)
                    .HasColumnName("Quantity")
                    .IsRequired();
            });

        // UnitPrice (Money) маппим как два поля: UnitPriceAmount, UnitPriceCurrency.
        builder.OwnsOne(
            x => x.UnitPrice,
            money =>
            {
                money.Property(x => x.Amount)
                    .HasColumnName("UnitPriceAmount")
                    .HasPrecision(18, 2)
                    .IsRequired();

                money.Property(x => x.Currency)
                    .HasColumnName("UnitPriceCurrency")
                    .HasMaxLength(3)
                    .IsRequired()
                    .HasConversion<CurrencyValueConverter>();
            });

        builder.ToTable("OrderItems");
    }
}