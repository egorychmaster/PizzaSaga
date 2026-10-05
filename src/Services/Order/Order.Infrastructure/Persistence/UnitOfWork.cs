using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using Order.Application.Abstractions.Persistence;
using Order.Application.Abstractions.Persistence.Idempotency.Exceptions;
using PizzaSaga.SharedKernel.Domain.DomainEvents;

namespace Order.Infrastructure.Persistence;

/// <summary>
/// Реализация IUnitOfWork для EF Core + PostgreSQL.
/// </summary>
public sealed class UnitOfWork : IUnitOfWork
{
    private readonly OrderDbContext _context;
    private readonly IDomainEventAccessor _domainEventAccessor;
    private readonly IDomainEventDispatcher _domainEventDispatcher;
    private readonly ILogger<UnitOfWork> _logger;

    public UnitOfWork(
        OrderDbContext context,
        IDomainEventAccessor domainEventAccessor,
        IDomainEventDispatcher domainEventDispatcher,
        ILogger<UnitOfWork> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _domainEventAccessor = domainEventAccessor ?? throw new ArgumentNullException(nameof(domainEventAccessor));
        _domainEventDispatcher = domainEventDispatcher ?? throw new ArgumentNullException(nameof(domainEventDispatcher));
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

                // 2. Извлекаем Domain Events из агрегатов, отслеживаемых текущим DbContext.
                var domainEvents = _domainEventAccessor.GetDomainEvents();
                if (domainEvents.Count > 0)
                {
                    // Преобразуем Domain Events в локальные действия Application.
                    // В случае OrderCreatedDomainEvent это приведёт к добавлению OrderCreatedIntegrationEvent в Outbox.
                    await _domainEventDispatcher.DispatchAsync(domainEvents, cancellationToken);

                    // События больше не должны повторно обрабатываться в рамках этого DbContext.
                    _domainEventAccessor.ClearDomainEvents();
                }

                // 3. Order, IdempotencyRecord и OutboxMessage сохраняются одним вызовом внутри одной транзакции.
                await _context.SaveChangesAsync(cancellationToken);

                // 4. Фиксируем транзакцию только после успешного SaveChanges.
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
