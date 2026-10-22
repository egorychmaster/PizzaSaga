using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Order.Infrastructure.MassTransit.Saga;

/// <summary>
/// Конфигурация хранения состояния Saga Order в PostgreSQL.
/// Использует отдельную таблицу OrderSagaStates, так как Saga — это process state, а не domain state.
/// </summary>
public sealed class OrderSagaStateMap : SagaClassMap<OrderSagaStateData>
{
    /// <inheritdoc />
    protected override void Configure(
        EntityTypeBuilder<OrderSagaStateData> builder,
        ModelBuilder model)
    {
        builder.ToTable("OrderSagaStates");

        builder.HasKey(x => x.CorrelationId);

        builder.Property(x => x.CurrentState)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(x => x.OrderId)
            .IsRequired();

        builder.Property(x => x.CustomerId)
            .IsRequired();

        builder.Property(x => x.TotalAmount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.CurrencyCode)
            .HasMaxLength(3)
            .IsRequired();

        // Optimistic Concurrency. Настройка xmin как токена конкурентности для PostgreSQL.
        builder.Property(x => x.Version).IsRowVersion();

        // Уникальный индекс по OrderId для быстрого поиска
        builder.HasIndex(x => x.OrderId)
            .IsUnique();
    }
}
