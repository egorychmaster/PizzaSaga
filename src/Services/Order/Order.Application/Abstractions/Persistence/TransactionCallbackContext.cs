namespace Order.Application.Abstractions.Persistence;

/// <summary>
/// Scoped-контекст callback-операций текущей команды.
/// Используется для передачи дополнительной работы во внутреннюю транзакционную область.
/// </summary>
public sealed class TransactionCallbackContext : ITransactionCallbackContext
{
    private Func<object?, CancellationToken, Task>? _callback;

    /// <inheritdoc />
    public void Register(Func<object?, CancellationToken, Task> callback)
    {
        ArgumentNullException.ThrowIfNull(callback);

        if (_callback is not null)
            throw new InvalidOperationException("Only one transaction callback can be registered for a command.");

        _callback = callback;
    }

    /// <inheritdoc />
    public async Task ExecuteAsync(object? result, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(result);

        if (_callback is null)
            return;

        await _callback(result, cancellationToken);
    }
}