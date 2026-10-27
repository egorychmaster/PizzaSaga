using Microsoft.Extensions.Options;
using Payment.Infrastructure.Configuration;

namespace Payment.Infrastructure.PaymentGateways;

/// <summary>
/// Имитирует результат авторизации платежа банковской системой.
/// Реализует логику симуляции отказа банка на основе конфигурации.
/// </summary>
public sealed class FakePaymentGateway(IOptions<PaymentSimulationOptions> options) : IPaymentGateway
{
    public Task<PaymentGatewayResult> AuthorizeAsync(
        Guid orderId,
        decimal amount,
        string currency,
        CancellationToken cancellationToken)
    {
        if (options.Value.SimulateFailure)
            // Создаёт результат отказа в авторизации.
            return Task.FromResult(PaymentGatewayResult.Declined("Bank authorization declined."));

        return Task.FromResult(PaymentGatewayResult.Approved());
    }
}
