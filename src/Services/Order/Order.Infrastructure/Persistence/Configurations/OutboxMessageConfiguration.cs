using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Order.Infrastructure.Persistence.Outbox;

namespace Order.Infrastructure.Persistence.Configurations;

/// <summary>
/// Конфигурация сущности OutboxMessage для EF Core.
/// </summary>
public sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        // Primary key
        builder.HasKey(x => x.Id);

        // Columns
        builder.Property(x => x.AggregateId).IsRequired();
        builder.Property(x => x.MessageType).IsRequired().HasMaxLength(256);
        builder.Property(x => x.Payload).IsRequired().HasColumnType("jsonb");
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.IsPublished).HasDefaultValue(false);

        // Maps to table
        builder.ToTable("OutboxMessages");
    }
}
