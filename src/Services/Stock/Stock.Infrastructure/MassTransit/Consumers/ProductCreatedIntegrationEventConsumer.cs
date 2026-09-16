using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PizzaSaga.Contracts.IntegrationEvents.Catalogs;
using Stock.Domain.AggregatesModel.Inventory;
using Stock.Infrastructure.Persistence;

namespace Stock.Infrastructure.MassTransit.Consumers;

/// <summary>
/// Consumer для интеграционного события ProductCreatedIntegrationEvent.
/// Создаёт запись остатка (Inventory) при добавлении нового продукта в Catalog Service.
/// </summary>
public sealed class ProductCreatedIntegrationEventConsumer : IConsumer<ProductCreatedIntegrationEvent>
{
    private readonly StockDbContext _context;
    private readonly ILogger<ProductCreatedIntegrationEventConsumer> _logger;

    public ProductCreatedIntegrationEventConsumer(StockDbContext context, ILogger<ProductCreatedIntegrationEventConsumer> logger)
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
            // Создаём запись остатка с начальным значением 100 единиц
            var inventory = InventoryAggregate.Create(@event.ProductId, availableQuantity: 100);

            _context.Inventories.Add(inventory);
            await _context.SaveChangesAsync(context.CancellationToken);

            _logger.LogInformation("Inventory created for ProductId={ProductId}, AvailableQuantity=100", @event.ProductId);
        }
        catch (DbUpdateException ex)
        {
            // Обработка дублирующихся записей — логируем и игнорируем
            _logger.LogWarning(ex, "Attempt to insert duplicate Inventory entry for ProductId={ProductId}. Skipping.", @event.ProductId);
        }
    }
}
