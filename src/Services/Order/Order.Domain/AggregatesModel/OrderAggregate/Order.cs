using Order.Domain.ValueObjects;

namespace Order.Domain.AggregatesModel.OrderAggregate;

public class Order
{
    public Guid Id { get; private set; }
    public string Status { get; private set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;

    public CustomerIdentity CustomerId { get; private set; }

    /// <summary>
    /// Версия агрегата для оптимистичной блокировки (Optimistic Concurrency).
    /// Инкрементируется при каждом изменении состояния агрегата.
    /// </summary>
    public int Version { get; private set; }


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
        // Начальная версия
        Version = 1; 
    }

    /// <summary>
    /// Фабрика для создания нового заказа.
    /// </summary>
    public static Order Create(Guid id, CustomerIdentity customerId)
        => new(id, customerId);

    ///// <summary>
    ///// Пример бизнес-метода изменения состояния агрегата.
    ///// Каждая модификация агрегата обязана инкрементировать версию.
    ///// </summary>
    //public void ChangeStatus(string newStatus)
    //{
    //    if (string.IsNullOrWhiteSpace(newStatus))
    //        throw new ArgumentException("Status cannot be empty.", nameof(newStatus));

    //    Status = newStatus;
    //    Version++;
    //}
}
