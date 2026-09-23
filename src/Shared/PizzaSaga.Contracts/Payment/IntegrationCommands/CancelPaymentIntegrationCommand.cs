namespace PizzaSaga.Contracts.Payment.IntegrationCommands;

/// <summary>
/// Команда для отмены платежа по заказу (возврат средств).
/// Обрабатывается Payment Service при компенсации.
/// </summary>
/// <param name="OrderId">Идентификатор заказа.</param>
public record CancelPaymentIntegrationCommand(
    Guid OrderId);
