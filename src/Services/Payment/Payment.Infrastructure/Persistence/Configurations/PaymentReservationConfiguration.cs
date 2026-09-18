using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Payment.Domain.AggregatesModel.PaymentReservation;
using Payment.Infrastructure.Persistence.Configurations.Converters;

namespace Payment.Infrastructure.Persistence.Configurations;

/// <summary>
/// Конфигурация сущности PaymentReservation для EF Core.
/// </summary>
public sealed class PaymentReservationConfiguration : IEntityTypeConfiguration<PaymentReservationAggregate>
{
    public void Configure(EntityTypeBuilder<PaymentReservationAggregate> builder)
    {
        // Primary key
        builder.HasKey(x => x.Id);

        // Columns
        builder.Property(x => x.OrderId).IsRequired();

        // Money маппим как два поля: TotalAmount, CurrencyCode
        builder.OwnsOne(x => x.TotalAmount, money =>
        {
            money.Property(x => x.Amount)
                .HasColumnName("TotalAmount")
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            money.Property(x => x.Currency)
                .HasColumnName("CurrencyCode")
                .HasMaxLength(3)
                .IsRequired()
                .HasConversion<CurrencyValueConverter>();
        });

        builder.Property(x => x.Version).IsConcurrencyToken(); // Optimistic Concurrency

        // Maps to table
        builder.ToTable("PaymentReservations");
    }
}
