using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderAggregate = Order.Domain.AggregatesModel.OrderAggregate.Order;

namespace Order.Infrastructure.Persistence.Configurations;

internal sealed class OrderConfiguration : IEntityTypeConfiguration<OrderAggregate>
{
    public void Configure(EntityTypeBuilder<OrderAggregate> builder)
    {
        // Primary key
        builder.HasKey(o => o.Id);

        builder.Property(o => o.Status).IsRequired();

        // Настройка CustomerId — CustomerIdentity как owned-тип (вложенный тип) в EF
        builder.OwnsOne(o => o.CustomerId, customer =>
        {
            // Это в sql: "CustomerId" UUID NOT NULL
            customer.Property(c => c.Value).HasColumnName("CustomerId").IsRequired();

            // Запрещаем использование конструктора — используем только свойство Value
            customer.UsePropertyAccessMode(PropertyAccessMode.Field);
        });


        // Maps to table
        builder.ToTable("orders");
    }
}
