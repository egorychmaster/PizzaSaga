using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderAggregate = Order.Domain.AggregatesModel.OrderAggregate.Order;

namespace Order.Infrastructure.Persistence.Configurations;

internal sealed class OrderConfiguration : IEntityTypeConfiguration<OrderAggregate>
{
    public void Configure(EntityTypeBuilder<OrderAggregate> builder)
    {
        builder.ToTable("orders");

        builder.HasKey(o => o.Id);

        // Настройка сопоставления полей, когда появится доменная модель Order
        // builder.Property(o => o.Status).IsRequired();
    }
}
