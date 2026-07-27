using System;
using System.Collections.Generic;
using System.Text;

namespace Order.Domain.AggregatesModel.OrderAggregate;

public class Order
{
    public Guid Id { get; private set; }
    public string Status { get; private set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;


    // Пустой конструктор для EF Core
    private Order() { }

    public Order(Guid id, string status)
    {
        Id = id;
        Status = status;
        CreatedAt = DateTimeOffset.UtcNow;
    }
}
