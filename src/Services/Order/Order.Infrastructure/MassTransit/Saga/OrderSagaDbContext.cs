using MassTransit.EntityFrameworkCoreIntegration;
using Microsoft.EntityFrameworkCore;

namespace Order.Infrastructure.MassTransit.Saga;

/// <summary>
/// DbContext для хранения экземпляров Saga Order.
/// Использует ту же PostgreSQL базу данных, что и Order Service (OrderDbContext).
/// Физически создаётся отдельная таблица OrderSagaStates.
/// </summary>
public sealed class OrderSagaDbContext : SagaDbContext
{
    /// <summary>
    /// Конструктор для EF Core.
    /// </summary>
    public OrderSagaDbContext(DbContextOptions<OrderSagaDbContext> options)
        : base(options)
    {
    }

    /// <inheritdoc />
    protected override IEnumerable<ISagaClassMap> Configurations
    {
        // Регистрация карты маппинга состояния саги в таблицу БД.
        get
        {
            yield return new OrderSagaStateMap();
        }
    }
}
