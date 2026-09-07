using Catalog.Domain.AggregatesModel.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Catalog.Infrastructure.Persistence.Configurations;

public class ProductAggregateConfiguration : IEntityTypeConfiguration<ProductAggregate>
{
    public void Configure(EntityTypeBuilder<ProductAggregate> builder)
    {
        // Primary key
        builder.HasKey(p => p.Id);

        // Columns
        builder.Property(p => p.Name).IsRequired().HasMaxLength(100);
        builder.Property(p => p.Description).HasMaxLength(500);

        //// Value object Price — owned entity
        // Owned-тип
        builder.OwnsOne(
            p => p.CurrentPrice,
            price =>
            {
                // Columns
                price.Property(p => p.Amount).HasColumnType("decimal(18,2)").IsRequired();
                price.Property(p => p.CurrencyCode).HasMaxLength(3).IsRequired();

                price.ToTable("Prices");
            }
            );

        // Maps to table
        builder.ToTable("Products");
    }
}