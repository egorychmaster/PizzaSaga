using Catalog.Application.Abstractions.DomainEvents;
using PizzaSaga.SharedKernel.Domain;

namespace Catalog.Infrastructure.Persistence.DomainEvents;

/// <summary>
/// Извлекает доменные события из агрегатов, отслеживаемых Entity Framework Core.
/// </summary>
public sealed class EfCoreDomainEventAccessor : IDomainEventAccessor
{
    private readonly CatalogDbContext _context;

    public EfCoreDomainEventAccessor(CatalogDbContext context)
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
