using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Order.Domain.AggregatesModel.Orders;

namespace Order.Infrastructure.Persistence.Configurations;

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

        builder.OwnsOne(
            x => x.Quantity,
            quantity =>
            {
                quantity.Property(x => x.Value)
                    .HasColumnName("Quantity")
                    .IsRequired();
            });

        // Money маппим как два поля: UnitPriceAmount, UnitPriceCurrency.
        builder.OwnsOne(
            x => x.UnitPrice,
            money =>
            {
                money.Property(x => x.Amount)
                    .HasColumnName("UnitPriceAmount")
                    .HasPrecision(18, 2)
                    .IsRequired();

                money.Property(x => x.CurrencyCode)
                    .HasColumnName("UnitPriceCurrency")
                    .HasMaxLength(3)
                    .IsRequired();
            });

        builder.ToTable("OrderItems");
    }
}