using Microsoft.EntityFrameworkCore;
using Order.Application.Abstractions.Persistence;
using Order.Domain.AggregatesModel.Orders;

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

    /// <inheritdoc />
    public async Task<OrderAggregate?> GetByIdAsync(Guid orderId, CancellationToken cancellationToken)
    {
        return await _context.Orders
            .AsNoTracking()
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<(IReadOnlyCollection<OrderAggregate>, long TotalCount)> GetListByCustomerIdAsync(Guid customerId, int page, int pageSize, CancellationToken cancellationToken)
    {
        var skip = (page - 1) * pageSize;

        var orders = await _context.Orders
            .AsNoTracking()
            .Where(o => o.CustomerId.Value == customerId)
            .OrderByDescending(o => o.CreatedAt)
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var totalCount = await _context.Orders
            .AsNoTracking()
            .Where(o => o.CustomerId.Value == customerId)
            .LongCountAsync(cancellationToken);

        return (orders, TotalCount: totalCount);
    }
}

