using MassTransit;
using Microsoft.Extensions.Logging;
using PizzaSaga.Contracts.Orders.IntegrationEvents;
using PizzaSaga.Contracts.Payment.IntegrationCommands;

namespace Payment.Infrastructure.MassTransit.CommandConsumers;

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
    public async Task Consume(ConsumeContext<AuthorizePaymentIntegrationCommand> context)
    {
        var message = context.Message;

        _logger.LogInformation(
            "Received AuthorizePayment for OrderId={OrderId}, Amount={Amount}, Currency={Currency}",
            message.OrderId,
            message.Amount,
            message.CurrencyCode);

        // На данном этапе — заглушка: логируем команду и публикуем успешное событие.
        // Полноценная логика авторизации будет реализована в Sprint 3.
        await context.Publish(new PaymentAuthorizedIntegrationEvent(message.OrderId));

        _logger.LogInformation(
            "PaymentAuthorizedIntegrationEvent published for OrderId={OrderId}",
            message.OrderId);
    }
}
