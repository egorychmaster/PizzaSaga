using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Order.Domain.ValueObjects;
using OrderAggregate = Order.Domain.AggregatesModel.OrderAggregate.Order;

namespace Order.Infrastructure.Persistence.Configurations;

internal sealed class OrderConfiguration : IEntityTypeConfiguration<OrderAggregate>
{
    public void Configure(EntityTypeBuilder<OrderAggregate> builder)
    {
        // Primary key
        builder.HasKey(o => o.Id);

        // Indexes
        //builder.HasIndex(o => o.CustomerId.Value);
        builder.HasIndex(o => o.Status);

        // Limit the size of columns to use efficient database types
        builder.Property(o => o.Status).HasConversion<string>().HasMaxLength(50).IsRequired();
        builder.Property(o => o.CreatedAt).IsRequired();
        // Оптимистичная блокировка (Optimistic Concurrency Control). EF Core при выполнении UPDATE будет добавлять условие: WHERE "Id" = @id AND "Version" = @oldVersion
        builder.Property(o => o.Version).IsConcurrencyToken().IsRequired();

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

                money.Property(x => x.CurrencyCode)
                    .HasColumnName("TotalCurrency")
                    .HasMaxLength(3)
                    .IsRequired();
            });

        // Relationships
        // Items — collection
        builder.HasMany(o => o.Items)
            .WithOne()
            .HasForeignKey("OrderId")
            .OnDelete(DeleteBehavior.Cascade);

        // Maps to table
        builder.ToTable("orders");
    }
}
