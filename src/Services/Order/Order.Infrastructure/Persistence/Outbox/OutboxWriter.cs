using Order.Application.Abstractions.Persistence.Outbox;
using System.Text.Json;

namespace Order.Infrastructure.Persistence.Outbox;

/// <summary>
/// Реализация IOutboxWriter для сохранения интеграционных событий в таблицу OutboxMessages.
/// Использует EF Core DbContext для добавления сущности и автоматического сохранения через SaveChangesAsync.
/// </summary>
public sealed class OutboxWriter : IOutboxWriter
{
    private readonly OrderDbContext _context;

    public OutboxWriter(OrderDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <inheritdoc />
    public void Add<TEvent>(Guid aggregateId, TEvent integrationEvent)
        where TEvent : class
    {
        if (integrationEvent is null)
            throw new ArgumentNullException(nameof(integrationEvent));

        // Создаём сущность OutboxMessage из интеграционного события через публичный конструктор
        var outboxMessageType = integrationEvent.GetType().AssemblyQualifiedName ?? integrationEvent.GetType().ToString();
        var outboxPayload = JsonSerializer.Serialize(integrationEvent, OutboxJsonSerializationOptionsProvider.Options);

        var outboxMessage = new OutboxMessage(
            aggregateId: aggregateId,
            messageType: outboxMessageType,
            payload: outboxPayload
        );
        // IsPublished по умолчанию false в конструкторе

        _context.OutboxMessages.Add(outboxMessage);
    }
}
