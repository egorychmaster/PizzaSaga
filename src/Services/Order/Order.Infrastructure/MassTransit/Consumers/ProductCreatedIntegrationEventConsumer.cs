using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Order.Domain.AggregatesModel.ProductCatalog;
using Order.Infrastructure.Persistence;
using PizzaSaga.Contracts.IntegrationEvents.Catalogs;

namespace Order.Infrastructure.MassTransit.Consumers;

/// <summary>
/// Consumer (потребитель) для интеграционного события ProductCreatedIntegrationEvent.
/// Обновляет локальный кэш продуктов при создании нового продукта в Catalog Service.
/// </summary>
public sealed class ProductCreatedIntegrationEventConsumer : IConsumer<ProductCreatedIntegrationEvent>
{
    private readonly OrderDbContext _context;
    private readonly ILogger<ProductCreatedIntegrationEventConsumer> _logger;

    public ProductCreatedIntegrationEventConsumer(OrderDbContext context, ILogger<ProductCreatedIntegrationEventConsumer> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task Consume(ConsumeContext<ProductCreatedIntegrationEvent> context)
    {
        var @event = context.Message;

        try
        {
            // Создаём запись кэша
            var productEntry = new ProductCatalogCache
            {
                ProductId = @event.ProductId,
                Name = @event.Name,
                Description = @event.Description,
                PriceAmount = @event.PriceAmount,
                CurrencyCode = @event.CurrencyCode
            };

            _context.ProductCatalog.Add(productEntry);
            await _context.SaveChangesAsync(context.CancellationToken);

            _logger.LogInformation(
                "Product catalog cache updated for ProductId={ProductId}, Name={Name}",
                @event.ProductId,
                @event.Name);
        }
        catch (DbUpdateException ex)
        {
            // Обработка дублирующихся записей — логируем и игнорируем
            _logger.LogWarning(
                ex,
                "Attempt to insert duplicate ProductCatalogCache entry for ProductId={ProductId}. Skipping.",
                @event.ProductId);
        }
    }
}
