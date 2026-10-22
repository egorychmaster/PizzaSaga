using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Stock.Domain.AggregatesModel.InventoryReservation;

namespace Stock.Infrastructure.Persistence.Configurations;

/// <summary>
/// Конфигурация сущности InventoryReservation для EF Core.
/// </summary>
public sealed class InventoryReservationConfiguration : IEntityTypeConfiguration<InventoryReservationAggregate>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<InventoryReservationAggregate> builder)
    {
        // Primary key
        builder.HasKey(x => x.Id);

        // Properties
        builder.Property(x => x.OrderId).IsRequired();

        // Unique index on OrderId to prevent duplicate reservations for the same order
        builder.HasIndex(x => x.OrderId).IsUnique();

        // Timestamps
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedAt);

        // Collection of items (owned type)
        builder.OwnsMany(x => x.Items, item =>
        {
            item.WithOwner().HasForeignKey("InventoryReservationId");
            item.Property<Guid>("InventoryReservationId").IsRequired();

            item.HasKey("InventoryReservationId", "ProductId"); // Пары уникальны

            item.Property(x => x.ProductId).IsRequired();
            item.Property(x => x.Quantity).IsRequired();
        });

        // Optimistic Concurrency. Настройка xmin как токена конкурентности для PostgreSQL.
        builder.Property(x => x.Version).IsRowVersion();

        // Map to table
        builder.ToTable("InventoryReservations");
    }
}
