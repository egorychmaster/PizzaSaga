using MassTransit;
using Microsoft.Extensions.Logging;
using PizzaSaga.Contracts.Stock.IntegrationCommands;
using PizzaSaga.Contracts.Stock.IntegrationEvents;

namespace Stock.Infrastructure.MassTransit.CommandConsumers;

/// <summary>
/// Consumer для обработки команды резервирования инвентаря.
/// На данном этапе просто логирует получение команды и публикует успешное событие.
/// </summary>
public sealed class ReserveInventoryConsumer : IConsumer<ReserveInventoryIntegrationCommand>
{
    private readonly ILogger<ReserveInventoryConsumer> _logger;

    public ReserveInventoryConsumer(ILogger<ReserveInventoryConsumer> logger)
    {
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

        // На данном этапе просто публикуем успешное событие.
        // Полноценная логика резервирования будет реализована в следующих шагах спринта.
        await context.Publish(new InventoryReservedIntegrationEvent(message.OrderId));

        _logger.LogInformation(
            "InventoryReservedIntegrationEvent published for OrderId={OrderId}",
            message.OrderId);
    }
}
