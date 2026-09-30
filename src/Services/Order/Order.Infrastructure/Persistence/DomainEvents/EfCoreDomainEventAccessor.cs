using Order.Application.Abstractions.DomainEvents;
using PizzaSaga.SharedKernel.Domain;

namespace Order.Infrastructure.Persistence.DomainEvents;

/// <summary>
/// Извлекает доменные события из агрегатов, отслеживаемых Entity Framework Core.
/// </summary>
public sealed class EfCoreDomainEventAccessor : IDomainEventAccessor
{
    private readonly OrderDbContext _context;

    public EfCoreDomainEventAccessor(OrderDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }
    
    /// <inheritdoc />
    public IReadOnlyCollection<IDomainEvent> GetDomainEvents()
    {
        return _context.ChangeTracker
            .Entries<AggregateRoot>()
            .SelectMany(entry => entry.Entity.DomainEvents)
            .ToArray();
    }

    /// <inheritdoc />
    public void ClearDomainEvents()
    {
        foreach (var entry in _context.ChangeTracker.Entries<AggregateRoot>())
        {
            entry.Entity.ClearDomainEvents();
        }
    }
}
