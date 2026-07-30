using Mediator;
using Order.Domain.ValueObjects;

namespace Order.Application.Features.Orders.CreateOrder;

/// <summary> 
/// Команда на создание нового заказа. 
/// Содержит identity авторизованного клиента и уже преобразованные значения, соответствующие доменным типам. 
/// </summary> 
/// <param name="CustomerId">Идентичность клиента, создающего заказ.</param> 
/// <param name="Items">Позиции создаваемого заказа.</param> 
/// <param name="PaymentMethod">Способ оплаты заказа.</param> 
/// <param name="Currency">Валюта заказа.</param>
public sealed record CreateOrderCommand(
    CustomerIdentity CustomerId,
    IReadOnlyCollection<CreateOrderItem> Items,
    string PaymentMethod,
    string Currency)
    : ICommand<CreateOrderResult>;

/// <summary> 
/// Позиция команды создания заказа. 
/// Количество представлено доменным Value Object <see cref="PizzaQuantity" />, 
/// поэтому его допустимый диапазон уже гарантирован при создании объекта. 
/// </summary> 
/// <param name="ProductId">Идентификатор продукта.</param> 
/// <param name="Quantity">Количество продукта в заказе.</param>
public sealed record CreateOrderItem(
    Guid ProductId,
    PizzaQuantity Quantity);