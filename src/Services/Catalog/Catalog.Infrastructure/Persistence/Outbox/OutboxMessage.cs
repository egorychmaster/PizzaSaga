namespace Catalog.Infrastructure.Persistence.Outbox;

/// <summary>
/// Сообщение Outbox для надёжной публикации интеграционных событий через RabbitMQ.
/// Гарантирует доставку интеграционных событий в другим сервисам даже при сбоях приложения, обеспечивая атомарность сохранения агрегата и события в одной транзакции.
/// </summary>
/// <remarks>
/// Сообщение не содержит доменные события — только интеграционные контракты.
/// После публикации в брокер, сообщение помечается как IsPublished = true и больше не отправляется.
/// </remarks>
public sealed class OutboxMessage
{
    /// <summary>
    /// Уникальный идентификатор сообщения.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Идентификатор агрегата, к которому относится интеграционное событие. Используется для отслеживания зависимости между агрегатом и событием.
    /// Например: Id ProductAggregate при создании продукта.
    /// </summary>
    public Guid AggregateId { get; set; }

    /// <summary>
    /// Полное имя типа интеграционного события. 
    /// Например: PizzaSaga.Contracts.IntegrationEvents.Catalogs.ProductCreatedIntegrationEvent, PizzaSaga.Contracts
    /// </summary>
    public string MessageType { get; set; } = null!;

    /// <summary>
    /// JSON-сериализованное представление интеграционного события.
    /// </summary>
    public string Payload { get; set; } = null!;

    /// <summary>
    /// Временная метка создания сообщения в UTC с учётом часового пояса.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// Флаг публикации: true — сообщение успешно отправлено в RabbitMQ, false — ожидает публикации.
    /// </summary>
    public bool IsPublished { get; set; }

    // EF Core
    private OutboxMessage() { }

    public OutboxMessage(Guid aggregateId, string messageType, string payload)
    {
        Id = Guid.NewGuid();
        AggregateId = aggregateId;
        MessageType = messageType;
        Payload = payload;
        CreatedAt = DateTimeOffset.UtcNow;
        IsPublished = false;
    }
}