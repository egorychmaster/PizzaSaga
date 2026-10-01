using MassTransit;
using Microsoft.EntityFrameworkCore;
using Payment.Domain.AggregatesModel.PaymentReservation;

namespace Payment.Infrastructure.Persistence;

public class PaymentDbContext : DbContext
{
    /// <summary>
    /// Платежные резервации.
    /// </summary>
    public DbSet<PaymentReservationAggregate> PaymentReservations => Set<PaymentReservationAggregate>();

    public PaymentDbContext(DbContextOptions<PaymentDbContext> options)
        : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Автоматическое применение всех конфигураций IEntityTypeConfiguration из текущей сборки.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PaymentDbContext).Assembly);

        // Регистрирует сущности Outbox/Inbox MassTransit в модели EF Core.
        modelBuilder.AddTransactionalOutboxEntities();
    }
}
