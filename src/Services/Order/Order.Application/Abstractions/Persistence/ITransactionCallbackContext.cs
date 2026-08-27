namespace Order.Application.Abstractions.Persistence;

/// <summary>
/// Контекст callback-операций, выполняемых внутри транзакции.
/// Callback вызываются после успешного выполнения обработчика, но до SaveChangesAsync и Commit.
/// </summary>
public interface ITransactionCallbackContext
{
    /// <summary>
    /// Регистрирует callback, который будет выполнен внутри текущей транзакции после успешного выполнения обработчика команды.
    /// </summary>
    /// <param name="callback">Операция, выполняемая перед SaveChangesAsync.</param>
    void Register(Func<object?, CancellationToken, Task> callback);

    /// <summary>
    /// Выполняет зарегистрированный callback.
    /// </summary>
    /// <param name="result">Результат выполнения обработчика команды.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    Task ExecuteAsync(object? result, CancellationToken cancellationToken);
}