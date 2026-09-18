using MassTransit;
using Microsoft.Extensions.Logging;
using PizzaSaga.Contracts.IntegrationEvents.Orders;

namespace Payment.Infrastructure.MassTransit.Consumers;

/// <summary>
/// Консьюмер команды AuthorizePaymentIntegrationCommand.
/// </summary>
public class AuthorizePaymentConsumer : IConsumer<AuthorizePaymentIntegrationCommand>
{
    private readonly ILogger<AuthorizePaymentConsumer> _logger;

    public AuthorizePaymentConsumer(ILogger<AuthorizePaymentConsumer> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public Task Consume(ConsumeContext<AuthorizePaymentIntegrationCommand> context)
    {
        _logger.LogInformation("Received AuthorizePayment for OrderId={OrderId}, Amount={Amount}", context.Message.OrderId, context.Message.Amount);

        // В Sprint 2 — ничего не делаем, только логируем.
        // В Sprint 3 добавим резервирование и публикацию событий.

        return Task.CompletedTask;
    }
}
