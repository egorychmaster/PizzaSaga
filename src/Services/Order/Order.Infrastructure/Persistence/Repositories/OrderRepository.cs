using Order.Application.Abstractions.Persistence;
using OrderAggregate = Order.Domain.AggregatesModel.OrderAggregate.Order;

namespace Order.Infrastructure.Persistence.Repositories;

/// <summary>
/// EF Core реализация репозитория агрегата Order.
/// </summary>
internal sealed class OrderRepository : IOrderRepository
{
    private readonly OrderDbContext _context;

    public OrderRepository(OrderDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <inheritdoc />
    public async Task AddAsync(OrderAggregate order, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(order);

        await _context.Orders.AddAsync(order, cancellationToken);
    }
}