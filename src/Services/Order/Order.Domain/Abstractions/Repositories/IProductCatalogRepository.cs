using Order.Domain.AggregatesModel.ProductCatalog;

namespace Order.Domain.Abstractions.Repositories;

/// <summary>
/// Репозиторий для работы с локальным кэшем каталога продуктов.
/// </summary>
public interface IProductCatalogRepository
{
    /// <summary>
    /// Получает продукт из кэша по идентификатору.
    /// </summary>
    /// <param name="productId">Идентификатор продукта.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Кэш-запись продукта или null, если не найден.</returns>
    Task<ProductCatalogCache?> GetProductAsync(Guid productId, CancellationToken cancellationToken);
}
