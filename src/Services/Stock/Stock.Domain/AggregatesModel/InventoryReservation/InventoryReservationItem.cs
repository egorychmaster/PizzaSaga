using System;

namespace Stock.Domain.AggregatesModel.InventoryReservation;

/// <summary>
/// Позиция резерва — конкретный продукт и его количество в рамках одного заказа.
/// Является дочерней сущностью агрегата InventoryReservation.
/// </summary>
public sealed class InventoryReservationItem
{
    /// <summary>
    /// Идентификатор продукта.
    /// </summary>
    public Guid ProductId { get; private set; } = default!;

    /// <summary>
    /// Количество для резервирования.
    /// </summary>
    public int Quantity { get; private set; }

    /// <summary>
    /// Конструктор для EF Core (для создания новых экземпляров используйте фабричный метод).
    /// </summary>
    private InventoryReservationItem() { }

    /// <summary>
    /// Создает новую позицию резерва.
    /// </summary>
    /// <param name="productId">Идентификатор продукта.</param>
    /// <param name="quantity">Количество для резервирования.</param>
    public InventoryReservationItem(Guid productId, int quantity)
    {
        ProductId = productId;
        Quantity = quantity;
    }
}
