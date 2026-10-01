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
        EntityTypeBuilder<OrderSagaStateData> entity,
        ModelBuilder model)
    {
        entity.ToTable("OrderSagaStates");

        entity.HasKey(x => x.CorrelationId);

        entity.Property(x => x.CurrentState)
            .HasMaxLength(64)
            .IsRequired();

        entity.Property(x => x.OrderId)
            .IsRequired();

        entity.Property(x => x.CustomerId)
            .IsRequired();

        entity.Property(x => x.TotalAmount)
            .HasPrecision(18, 2)
            .IsRequired();

        entity.Property(x => x.CurrencyCode)
            .HasMaxLength(3)
            .IsRequired();

        // Настройка xmin как токена конкурентности для PostgreSQL
        entity.Property(x => x.Version)
              .HasColumnName("xmin")
              .HasColumnType("xid")
              .ValueGeneratedOnAddOrUpdate()
              .IsConcurrencyToken();

        // Уникальный индекс по OrderId для быстрого поиска
        entity.HasIndex(x => x.OrderId)
            .IsUnique();
    }
}
