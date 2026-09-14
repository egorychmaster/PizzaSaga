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

    /// <summary>
    /// Получает заказ по идентификатору.
    /// </summary>
    /// <param name="orderId">Идентификатор заказа.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Заказ или null, если не найден.</returns>
    Task<OrderAggregate?> GetByIdAsync(Guid orderId, CancellationToken cancellationToken);

    /// <summary>
    /// Получает страницу заказов пользователя.
    /// </summary>
    /// <param name="customerId">Идентификатор клиента.</param>
    /// <param name="page">Номер страницы (1-based).</param>
    /// <param name="pageSize">Размер страницы.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Кортеж: коллекция заказов на текущей странице и общее количество заказов.</returns>
    Task<(IReadOnlyCollection<OrderAggregate>, long TotalCount)> GetListByCustomerIdAsync(Guid customerId, int page, int pageSize, CancellationToken cancellationToken);
}