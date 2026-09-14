using Mediator;
using Order.Application.Abstractions.Persistence;
using Order.Domain.AggregatesModel.Orders.Exceptions;

namespace Order.Application.Features.Orders.GetOrderById;

/// <summary>
/// Обработчик запроса на получение детализации заказа.
/// </summary>
public sealed class GetOrderByIdQueryHandler : IQueryHandler<GetOrderByIdQuery, GetOrderByIdResult>
{
    private readonly IOrderRepository _orderRepository;

    /// <summary>
    /// Создаёт экземпляр обработчика.
    /// </summary>
    /// <param name="orderRepository">Репозиторий заказов.</param>
    public GetOrderByIdQueryHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
    }

    /// <inheritdoc />
    public async ValueTask<GetOrderByIdResult> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        // Получаем заказ из базы данных
        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
        if (order is null)
            throw new OrderNotFoundException(request.OrderId);

        // Преобразуем доменную модель в DTO
        var items = order.Items.Select(item => new GetOrderItemResult(
            ProductId: item.ProductId,
            Quantity: item.Quantity.Value,
            UnitPrice: item.UnitPrice.Amount)).ToArray();

        var result = new GetOrderByIdResult(
            OrderId: order.Id,
            CustomerId: order.CustomerId.Value,
            Status: order.Status.ToString(),
            Items: items,
            TotalAmount: order.TotalAmount.Amount,
            Currency: order.TotalAmount.Currency.Code,
            CreatedAt: order.CreatedAt,
            UpdatedAt: order.CreatedAt
        );

        return result;
    }
}
