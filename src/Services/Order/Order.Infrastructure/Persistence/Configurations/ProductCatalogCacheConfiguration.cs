using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Order.Domain.AggregatesModel.ProductCatalog;

namespace Order.Infrastructure.Persistence.Configurations;

/// <summary>
/// Конфигурация сущности для Entity Framework Core.
/// </summary>
public sealed class ProductCatalogCacheConfiguration : IEntityTypeConfiguration<ProductCatalogCache>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ProductCatalogCache> builder)
    {
        // Указываем имя таблицы
        builder.ToTable("ProductCatalogCaches");

        // Устанавливаем Primary Key
        builder.HasKey(e => e.ProductId);

        // Columns
        builder.Property(e => e.ProductId)
            .ValueGeneratedNever()
            .IsRequired();

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.Description)
            .IsRequired(false)
            .HasMaxLength(1000);

        builder.Property(e => e.PriceAmount)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(e => e.CurrencyCode)
            .IsRequired()
            .HasMaxLength(3);
    }
}
