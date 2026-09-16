using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Stock.Domain.AggregatesModel.Inventory;

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
        builder.Property(x => x.AvailableQuantity).IsRequired();
        builder.Property(x => x.ReservedQuantity).IsRequired();

        // Maps to table
        builder.ToTable("Inventories");
    }
}
