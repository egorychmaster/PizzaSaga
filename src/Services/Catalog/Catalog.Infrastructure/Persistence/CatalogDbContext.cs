using Catalog.Domain.AggregatesModel.Products;
using Catalog.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Catalog.Infrastructure.Persistence;

public class CatalogDbContext : DbContext
{
    public DbSet<ProductAggregate> Products => Set<ProductAggregate>();

    public CatalogDbContext(DbContextOptions<CatalogDbContext> options)
        : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new ProductAggregateConfiguration());

        // Автоматическое применение всех конфигураций IEntityTypeConfiguration из текущей сборки (Catalog.Infrastructure)
        //modelBuilder.ApplyConfigurationsFromAssembly(typeof(CatalogDbContext).Assembly);
    }
}