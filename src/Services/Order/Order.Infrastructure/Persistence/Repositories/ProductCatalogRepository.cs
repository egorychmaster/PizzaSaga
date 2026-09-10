using Microsoft.EntityFrameworkCore;
using Order.Domain.Abstractions.Repositories;
using Order.Domain.AggregatesModel.ProductCatalog;

namespace Order.Infrastructure.Persistence.Repositories;

/// <summary>
/// Реализация репозитория для работы с локальным кэшем каталога продуктов.
/// </summary>
public sealed class ProductCatalogRepository : IProductCatalogRepository
{
    private readonly OrderDbContext _context;

    /// <summary>
    /// Создаёт экземпляр репозитория.
    /// </summary>
    public ProductCatalogRepository(OrderDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <inheritdoc />
    public async Task<ProductCatalogCache?> GetProductAsync(Guid productId, CancellationToken cancellationToken)
    {
        return await _context.ProductCatalog
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.ProductId == productId, cancellationToken);
    }
}
