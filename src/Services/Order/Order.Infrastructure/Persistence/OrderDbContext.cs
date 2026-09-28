using Microsoft.EntityFrameworkCore;
using Order.Domain.AggregatesModel.Orders;
using Order.Domain.AggregatesModel.Orders.ValueObjects;
using Order.Domain.AggregatesModel.ProductCatalog;
using Order.Infrastructure.Persistence.Idempotency;
using Order.Infrastructure.Persistence.Outbox;

namespace Order.Infrastructure.Persistence;

public sealed class OrderDbContext : DbContext
{
    public OrderDbContext(DbContextOptions<OrderDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Сущности/агрегата Заказа.
    /// </summary>
    public DbSet<OrderAggregate> Orders => Set<OrderAggregate>();

    /// <summary>
    /// Кэш продуктов из Catalog Service.
    /// </summary>
    public DbSet<ProductCatalogCache> ProductCatalog => Set<ProductCatalogCache>();

    /// <summary>
    /// Набор записей идемпотентности HTTP-запросов.
    /// </summary>
    public DbSet<IdempotencyRecord> IdempotencyRecords => Set<IdempotencyRecord>();

    /// <summary>
    /// Курсы конвертации валют.
    /// </summary>
    public DbSet<CurrencyExchangeRate> CurrencyExchangeRates => Set<CurrencyExchangeRate>();

    /// <summary>
    /// Сообщения Outbox для надёжной публикации интеграционных событий.
    /// </summary>
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Автоматическое применение всех конфигураций IEntityTypeConfiguration из текущей сборки.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OrderDbContext).Assembly);
    }
}
