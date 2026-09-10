using Catalog.Domain.AggregatesModel.Products;
using Catalog.Infrastructure.Persistence.Configurations;
using Catalog.Infrastructure.Persistence.Outbox;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Infrastructure.Persistence;

public class CatalogDbContext : DbContext
{
    public DbSet<ProductAggregate> Products => Set<ProductAggregate>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    public CatalogDbContext(DbContextOptions<CatalogDbContext> options)
        : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new ProductAggregateConfiguration());
        modelBuilder.ApplyConfiguration(new OutboxMessageConfiguration());

        // Автоматическое применение всех конфигураций IEntityTypeConfiguration из текущей сборки (Catalog.Infrastructure)
        //modelBuilder.ApplyConfigurationsFromAssembly(typeof(CatalogDbContext).Assembly);
    }
}