using Microsoft.EntityFrameworkCore;
using PizzaSaga.SharedKernel.Domain;
using PizzaSaga.SharedKernel.Domain.DomainEvents;

namespace PizzaSaga.Shared.Infrastructure.Persistence.DomainEvents;

/// <summary>
/// Обобщённая реализация IDomainEventAccessor для работы с Entity Framework Core контекстами.
/// </summary>
/// <typeparam name="TContext">Тип DbContext, от которого наследуются агрегаты.</typeparam>
public sealed class EfCoreDomainEventAccessor<TContext> : IDomainEventAccessor
    where TContext : DbContext
{
    private readonly TContext _context;

    public EfCoreDomainEventAccessor(TContext context)
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
