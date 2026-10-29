using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Order.Domain.AggregatesModel.Orders;
using Order.Infrastructure.Persistence.Configurations.Converters;

namespace Order.Infrastructure.Persistence.Configurations;

/// <summary>
/// Конфигурация сущности для Entity Framework Core.
/// </summary>
internal sealed class OrderAggregateConfiguration : IEntityTypeConfiguration<OrderAggregate>
{
    public void Configure(EntityTypeBuilder<OrderAggregate> builder)
    {
        // Primary key
        builder.HasKey(o => o.Id);

        // Indexes
        //builder.HasIndex(o => o.CustomerId.Value);
        builder.HasIndex(o => o.Status);

        // Columns
        builder.Property(o => o.Status).HasConversion<string>().HasMaxLength(50).IsRequired();
        builder.Property(o => o.CreatedAt).IsRequired();
        // Optimistic Concurrency. Настройка xmin как токена конкурентности для PostgreSQL.
        // Значение поддерживается базой данных, домен его не изменяет.
        builder.Property(o => o.Version).IsRowVersion();

        // Настройка CustomerId — CustomerIdentity как owned-тип (вложенный тип) в EF.
        builder.OwnsOne(o => o.CustomerId, customer =>
        {
            // Это в sql: "CustomerId" UUID NOT NULL
            customer.Property(c => c.Value).HasColumnName("CustomerId").IsRequired();

            // Запрещаем использование конструктора — используем только свойство Value
            customer.UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        // Money маппим как два поля: TotalAmount, TotalCurrency.
        builder.OwnsOne(
            o => o.TotalAmount,
            money =>
            {
                money.Property(x => x.Amount)
                    .HasColumnName("TotalAmount")
                    .HasPrecision(18, 2)
                    .IsRequired();

                money.Property(x => x.Currency)
                    .HasColumnName("TotalCurrency")
                    .HasMaxLength(3)
                    .IsRequired()
                    .HasConversion<CurrencyValueConverter>();
            });

        // Relationships
        // Items — collection
        builder.HasMany(o => o.Items)
            .WithOne()
            .HasForeignKey("OrderId")
            .OnDelete(DeleteBehavior.Cascade);

        // Maps to table
        builder.ToTable("Orders");
    }
}
