using Order.Domain.AggregatesModel.Orders;

namespace Order.Application.Abstractions.Persistence;

/// <summary>
/// Репозиторий агрегата Order.
/// </summary>
public interface IOrderRepository
{
    /// <summary>
    /// Добавляет новый заказ в текущий Unit of Work.
    /// </summary>
    Task AddAsync(OrderAggregate order, CancellationToken cancellationToken = default);
}