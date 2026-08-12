using Order.Application.Abstractions.Persistence;
using System;
using System.Collections.Generic;
using System.Text;

namespace Order.Infrastructure.Persistence.Repositories;

public sealed class OrderRepository : IOrderRepository
{
    private readonly OrderDbContext _context;

    public OrderRepository(OrderDbContext context) => _context = context ?? throw new ArgumentNullException(nameof(context));

    //public async Task AddAsync(Order order, CancellationToken cancellationToken = default)
    //{
    //    await _context.Orders.AddAsync(order, cancellationToken);
    //}
}