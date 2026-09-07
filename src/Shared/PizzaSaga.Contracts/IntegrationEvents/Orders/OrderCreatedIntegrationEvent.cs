namespace PizzaSaga.Contracts.IntegrationEvents.Orders;

/// <summary>
/// Интеграционное событие создания заказа, публикуемое в брокер/Outbox.
/// </summary>
public sealed record OrderCreatedIntegrationEvent(
    Guid OrderId,
    Guid CustomerId,
    decimal TotalAmount,
    string Currency,
    DateTimeOffset CreatedAt);
