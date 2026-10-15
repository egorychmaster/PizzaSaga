using System.Threading.Tasks;

namespace Stock.Domain.AggregatesModel.Inventory;

/// <summary>
/// Репозиторий для работы с инвентарем (остатками).
/// </summary>
public interface IInventoryRepository
{
    /// <summary>
    /// Находит инвентарь по идентификатору продукта.
    /// </summary>
    /// <param name="productId">Идентификатор продукта.</param>
    /// <param name="ct">Токен отмены.</param>
    /// <returns>Экземпляр инвентаря или null, если не найден.</returns>
    Task<InventoryAggregate?> GetByIdAsync(Guid productId, CancellationToken ct = default);

    /// <summary>
    /// Находит несколько инвентарей по списку идентификаторов продуктов.
    /// </summary>
    /// <param name="productIds">Список идентификаторов продуктов.</param>
    /// <param name="ct">Токен отмены.</param>
    /// <returns>Коллекция инвентарей.</returns>
    Task<IEnumerable<InventoryAggregate>> GetByIdsAsync(Guid[] productIds, CancellationToken ct = default);

    /// <summary>
    /// Добавляет новый инвентарь.
    /// </summary>
    /// <param name="inventory">Инвентарь для добавления.</param>
    /// <param name="ct">Токен отмены.</param>
    Task AddAsync(InventoryAggregate inventory, CancellationToken ct = default);

    /// <summary>
    /// Обновляет существующий инвентарь.
    /// </summary>
    /// <param name="inventory">Инвентарь для обновления.</param>
    /// <param name="ct">Токен отмены.</param>
    Task UpdateAsync(InventoryAggregate inventory, CancellationToken ct = default);
}
