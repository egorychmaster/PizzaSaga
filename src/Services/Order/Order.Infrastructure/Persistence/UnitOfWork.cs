using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using Order.Application.Abstractions.Persistence;
using Order.Application.Abstractions.Persistence.Idempotency.Exceptions;
using Order.Infrastructure.Persistence.Outbox;
using System.Text.Json;

namespace Order.Infrastructure.Persistence;

/// <summary>
/// Реализация IUnitOfWork для EF Core + PostgreSQL.
/// </summary>
public sealed class UnitOfWork : IUnitOfWork
{
    private readonly OrderDbContext _context;
    private readonly ILogger<UnitOfWork> _logger;

    public UnitOfWork(OrderDbContext context, ILogger<UnitOfWork> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<TResponse> ExecuteInTransactionAsync<TResponse>(Func<CancellationToken, Task<TResponse>> action, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(action);

        _logger.LogTrace("Starting transaction execution.");

        // Создаем стратегию повторных попыток Npgsql / EF Core
        var strategy = _context.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            // Просто начинаем новую транзакцию (вложенность не поддерживается)
            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                // 1. Выполняем бизнес-логику (Application Handler)
                var result = await action(cancellationToken);

                // 2. Сохраняем изменения, выполненные Handler
                await _context.SaveChangesAsync(cancellationToken);

                // 3. Фиксируем транзакцию только после успешного SaveChanges.
                await transaction.CommitAsync(cancellationToken);

                _logger.LogTrace("Transaction completed successfully.");

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Transaction failed, rolling back.");

                // При ошибке Handler или SaveChanges откатываем транзакцию.
                try
                {
                    await transaction.RollbackAsync(cancellationToken);
                }
                catch (Exception rollbackEx)
                {
                    _logger.LogError(rollbackEx, "Rollback failed.");
                }

                if (ex is DbUpdateException dbUpdateException && IsDuplicateIdempotencyKey(dbUpdateException))
                {
                    throw new DuplicateIdempotencyKeyException(ex);
                }

                throw;
            }
        });
    }

    /// <inheritdoc />
    public async Task SaveWithOutboxAsync<TEvent>(Guid aggregateId, TEvent integrationEvent, CancellationToken cancellationToken = default)
        where TEvent : class
    {
        ArgumentNullException.ThrowIfNull(integrationEvent);

        _logger.LogTrace("Saving Outbox message for aggregate {AggregateId} with event type {EventType}.", aggregateId, typeof(TEvent).Name);

        // Сериализуем событие в JSON
        var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var payload = JsonSerializer.Serialize(integrationEvent, integrationEvent.GetType(), jsonOptions);

        // Формируем полное имя типа для десериализации
        var messageType = $"{integrationEvent.GetType().FullName}, {integrationEvent.GetType().Assembly.GetName().Name}";

        // Добавляем сообщение в Outbox — оно будет сохранено вместе с другими изменениями DbContext
        _context.OutboxMessages.Add(new OutboxMessage(
            aggregateId: aggregateId,
            messageType: messageType,
            payload: payload));

        // Сохраняем Outbox-сообщение (в рамках текущей транзакции)
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogTrace("Outbox message saved for aggregate {AggregateId}.", aggregateId);
    }

    /// <summary>
    /// Проверяет, является ли DbUpdateException результатом нарушения уникального ограничения Idempotency-Key.
    /// </summary>
    private static bool IsDuplicateIdempotencyKey(DbUpdateException exception)
    {
        return exception.InnerException is PostgresException postgresException
               && postgresException.SqlState == PostgresErrorCodes.UniqueViolation
               && postgresException.ConstraintName is not null
               && postgresException.ConstraintName.Contains(
                   "idempotency",
                   StringComparison.OrdinalIgnoreCase);
    }
}
