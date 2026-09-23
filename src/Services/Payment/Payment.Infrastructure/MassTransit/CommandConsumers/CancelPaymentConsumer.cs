using MassTransit;
using Microsoft.Extensions.Logging;
using PizzaSaga.Contracts.Payment.IntegrationCommands;

namespace Payment.Infrastructure.MassTransit.CommandConsumers;

/// <summary>
/// Консьюмер команды CancelPaymentIntegrationCommand.
/// </summary>
public class CancelPaymentConsumer : IConsumer<CancelPaymentIntegrationCommand>
{
    private readonly ILogger<CancelPaymentConsumer> _logger;

    public CancelPaymentConsumer(ILogger<CancelPaymentConsumer> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public Task Consume(ConsumeContext<CancelPaymentIntegrationCommand> context)
    {
        _logger.LogInformation("Received CancelPayment for OrderId={OrderId}", context.Message.OrderId);

        // В Sprint 2 — ничего не делаем, только логируем.
        // В Sprint 3 добавим отмену резервирования.

        return Task.CompletedTask;
    }
}
