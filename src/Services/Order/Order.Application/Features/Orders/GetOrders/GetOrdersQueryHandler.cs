using Mediator;
using Order.Application.Abstractions.Persistence;

namespace Order.Application.Features.Orders.GetOrders;

/// <summary>
/// Обработчик запроса на получение списка заказов текущего пользователя.
/// </summary>
public sealed class GetOrdersQueryHandler : IQueryHandler<GetOrdersQuery, GetOrdersResult>
{
    private readonly IOrderRepository _orderRepository;

    /// <summary>
    /// Создаёт экземпляр обработчика.
    /// </summary>
    /// <param name="orderRepository">Репозиторий заказов.</param>
    public GetOrdersQueryHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
    }

    /// <inheritdoc />
    public async ValueTask<GetOrdersResult> Handle(GetOrdersQuery request, CancellationToken cancellationToken)
    {
        // Получаем заказы пользователя из базы данных (кортеж: список заказов и общее количество)
        var (orders, totalCount) = await _orderRepository.GetListByCustomerIdAsync(request.CustomerId, request.Page, request.PageSize, cancellationToken);

        var items = orders.Select(order => new GetOrdersItemResult(
            OrderId: order.Id,
            Status: order.Status.ToString(),
            CreatedAt: order.CreatedAt)).ToList();

        var results = new GetOrdersResult(
            Items: items,
            Page: request.Page,
            PageSize: request.PageSize,
            TotalCount: totalCount
        );

        return results;
    }
}