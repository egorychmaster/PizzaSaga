using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Stock.Domain.AggregatesModel.Inventory;
using Stock.Infrastructure.Persistence.Configurations.ValueConverters;

namespace Stock.Infrastructure.Persistence.Configurations;

/// <summary>
/// Конфигурация сущности Inventory для EF Core.
/// </summary>
public sealed class InventoryConfiguration : IEntityTypeConfiguration<InventoryAggregate>
{
    public void Configure(EntityTypeBuilder<InventoryAggregate> builder)
    {
        // Primary key
        builder.HasKey(x => x.ProductId);

        // Columns
        builder.Property(x => x.ProductId).IsRequired();
        
        // Map InventoryBalance properties to columns with value converters
        builder.OwnsOne(x => x.Balance, balance =>
        {
            var converter = QuantityConverter.Create();

            balance.Property(b => b.Available)
                .HasColumnName("AvailableQuantity")
                .HasConversion(converter)
                .ValueGeneratedNever()
                .IsRequired();

            balance.Property(b => b.Reserved)
                .HasColumnName("ReservedQuantity")
                .HasConversion(converter)
                .ValueGeneratedNever()
                .IsRequired();
        });

        // Maps to table
        builder.ToTable("Inventories");
    }
}
