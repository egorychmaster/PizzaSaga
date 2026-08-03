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

        // index

        // Limit the size of columns to use efficient database types
        builder.Property(o => o.Status).HasMaxLength(50).IsRequired();
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


        // Relationships


        // Maps to table
        builder.ToTable("orders");
    }
}
