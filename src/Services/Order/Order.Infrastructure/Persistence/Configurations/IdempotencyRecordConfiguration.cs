using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Order.Infrastructure.Persistence.Idempotency;

namespace Order.Infrastructure.Persistence.Configurations;

/// <summary>
/// Настройки маппинга сущности IdempotencyKey для EF Core.
/// Гарантирует, что ResponseBody будет храниться как PostgreSQL jsonb.
/// </summary>
internal sealed class IdempotencyRecordConfiguration : IEntityTypeConfiguration<IdempotencyRecord>
{
    public void Configure(EntityTypeBuilder<IdempotencyRecord> builder)
    {
        // Primary key
        builder.HasKey(x => x.IdempotencyKey);

        // Limit the size of columns to use efficient database types
        builder.Property(x => x.IdempotencyKey).ValueGeneratedNever();
        builder.Property(x => x.RequestHash).IsRequired()
            .HasMaxLength(64); // SHA-256 в hex-представлении — 64 символа
        //builder.Property(x => x.Status).IsRequired().HasConversion<int>();
        builder.Property(x => x.ResponseStatusCode).IsRequired();
        builder.Property(x => x.ResponseBody).IsRequired().HasColumnType("jsonb");
        builder.Property(x => x.CreatedAt).IsRequired();

        // Maps to table
        builder.ToTable("idempotency_records");
    }
}
