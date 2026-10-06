using MassTransit;
using Microsoft.Extensions.Logging;
using PizzaSaga.Contracts.Stock.IntegrationCommands;
using PizzaSaga.Contracts.Stock.IntegrationEvents;
using Stock.Infrastructure.Persistence;

namespace Stock.Infrastructure.MassTransit.CommandConsumers;

/// <summary>
/// Consumer для обработки команды освобождения инвентаря.
/// Вызывается при отмене заказа или при необходимости вернуть товар на склад.
/// </summary>
public sealed class ReleaseInventoryConsumer : IConsumer<ReleaseInventoryIntegrationCommand>
{
    private readonly ILogger<ReleaseInventoryConsumer> _logger;

    public ReleaseInventoryConsumer(ILogger<ReleaseInventoryConsumer> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task Consume(ConsumeContext<ReleaseInventoryIntegrationCommand> context)
    {
        var message = context.Message;

        _logger.LogInformation(
            "ReleaseInventoryIntegrationCommand received for OrderId={OrderId}",
            message.OrderId);

        // 1. Изменяем состояние домена/БД
        //var inventory = await _dbContext.Inventories.FindAsync(message.OrderId);
        //inventory.Release(message.Products);

        // 2. Публикуем событие
        // На данном этапе просто публикуем успешное событие.
        // Полноценная логика освобождения будет реализована в следующих шагах спринта.
        await context.Publish(new InventoryReleasedIntegrationEvent(message.OrderId));

        _logger.LogInformation("InventoryReleasedIntegrationEvent published for OrderId={OrderId}", message.OrderId);
    }
}
