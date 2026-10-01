using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PizzaSaga.Contracts.Stock.IntegrationCommands;
using PizzaSaga.Contracts.Stock.IntegrationEvents;
using Stock.Infrastructure.Persistence;

namespace Stock.Infrastructure.MassTransit.CommandConsumers;

/// <summary>
/// Consumer для обработки команды резервирования инвентаря.
/// На данном этапе просто логирует получение команды и публикует успешное событие.
/// </summary>
public sealed class ReserveInventoryConsumer : IConsumer<ReserveInventoryIntegrationCommand>
{
    //private readonly StockDbContext _dbContext;
    private readonly ILogger<ReserveInventoryConsumer> _logger;

    public ReserveInventoryConsumer(/*StockDbContext dbContext,*/ ILogger<ReserveInventoryConsumer> logger)
    {
        //_dbContext = dbContext;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task Consume(ConsumeContext<ReserveInventoryIntegrationCommand> context)
    {
        var message = context.Message;

        _logger.LogInformation(
            "ReserveInventoryIntegrationCommand received for OrderId={OrderId}, ProductsCount={ProductsCount}",
            message.OrderId,
            message.Products?.Count() ?? 0);

        // 1. Изменяем состояние домена/БД
        //var inventory = await _dbContext.Inventories.FindAsync(message.Products.);
        //inventory.Reserve(message.Products);

        // 2. Публикуем событие
        // На данном этапе просто публикуем успешное событие.
        // Полноценная логика резервирования будет реализована в следующих шагах спринта.
        await context.Publish(new InventoryReservedIntegrationEvent(message.OrderId));

        // 3. Коммитим БД. MassTransit перехватит этот момент и запишет событие в OutboxMessage
        //await _dbContext.SaveChangesAsync(context.CancellationToken);

        _logger.LogInformation("InventoryReservedIntegrationEvent published for OrderId={OrderId}", message.OrderId);
    }
}
