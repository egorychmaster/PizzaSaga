using Order.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace Order.Domain.AggregatesModel.OrderAggregate;

public class Order
{
    public Guid Id { get; private set; }
    public string Status { get; private set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;

    public CustomerIdentity CustomerId { get; private set; }

    // Пустой конструктор для EF Core
    private Order() { }

    /// <summary>
    /// Создаёт новый заказ с указанным идентификатором и клиентом.
    /// </summary>
    public Order(Guid id, CustomerIdentity customerId)
    {
        Id = id;
        CustomerId = customerId ?? throw new ArgumentNullException(nameof(customerId));
        Status = "Pending";
        CreatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Фабрика для создания нового заказа.
    /// </summary>
    public static Order Create(Guid id, CustomerIdentity customerId)
        => new(id, customerId);
}
