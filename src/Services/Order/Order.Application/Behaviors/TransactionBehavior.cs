using Mediator;
using Microsoft.Extensions.Logging;
using Order.Application.Abstractions.Persistence;

namespace Order.Application.Behaviors;

/// <summary>
/// Pipeline Behavior для автоматического управления транзакциями БД.
/// Применяется строго к командам (ICommand<TResponse>).
/// </summary>
public sealed class TransactionBehavior<TMessage, TResponse> : IPipelineBehavior<TMessage, TResponse>
    where TMessage : ICommand<TResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<TransactionBehavior<TMessage, TResponse>> _logger;

    public TransactionBehavior(IUnitOfWork unitOfWork, ILogger<TransactionBehavior<TMessage, TResponse>> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async ValueTask<TResponse> Handle(TMessage message, MessageHandlerDelegate<TMessage, TResponse> next, CancellationToken cancellationToken)
    {
        var commandName = typeof(TMessage).Name;

        try
        {
            _logger.LogInformation("Executing command {CommandName} inside transaction", commandName);

            // Вся магия транзакций, SaveChanges, Rollback и Retry Strategy прозрачно выполняется внутри UnitOfWork
            return await _unitOfWork.ExecuteInTransactionAsync(
                async ct => await next(message, ct),
                cancellationToken);
        }
        catch (OperationCanceledException)
        {
            _logger.LogDebug("Command {CommandName} was cancelled", commandName);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Command {CommandName} failed", commandName);
            throw;
        }
    }
}