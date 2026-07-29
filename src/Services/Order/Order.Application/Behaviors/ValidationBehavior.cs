using FluentValidation;
using Mediator;

namespace Order.Application.Behaviors;

/// <summary> 
/// Pipeline Behavior для автоматической валидации команд перед выполнением Handler. 
/// </summary> 
/// <typeparam name="TMessage">Тип команды.</typeparam> 
/// <typeparam name="TResponse">Тип результата команды.</typeparam> 
/// <remarks> 
/// Behavior применяется только к сообщениям, реализующим <see cref="ICommand{TResponse}"/>. 
/// Поэтому Query-запросы не проходят через данный Behavior. 
/// Ответственность Behavior ограничена запуском FluentValidation. 
/// Бизнес-инварианты домена здесь не проверяются.
/// </remarks>
public sealed class ValidationBehavior<TMessage, TResponse>
    : IPipelineBehavior<TMessage, TResponse>
    where TMessage : ICommand<TResponse>
{
    private readonly IEnumerable<IValidator<TMessage>> _validators;

    /// <summary> 
    /// Создаёт экземпляр ValidationBehavior. 
    /// </summary> 
    /// <param name="validators"> 
    /// Все FluentValidation-валидаторы, зарегистрированные для текущей команды. 
    /// </param>
    public ValidationBehavior(IEnumerable<IValidator<TMessage>> validators)
    {
        _validators = validators;
    }

    /// <summary> 
    /// Выполняет валидацию команды перед передачей управления следующему элементу pipeline. 
    /// </summary> 
    /// <param name="message">Команда, поступившая в pipeline.</param> 
    /// <param name="next"> 
    /// Следующий элемент pipeline. Если validation успешна, через него будет вызван Handler. 
    /// </param> 
    /// <param name="cancellationToken">Токен отмены операции.</param> 
    /// <returns>Результат выполнения команды.</returns> 
    /// <exception cref="ValidationException"> 
    /// Выбрасывается, если FluentValidation обнаружил одну или несколько ошибок. 
    /// </exception>
    public async ValueTask<TResponse> Handle(TMessage message, MessageHandlerDelegate<TMessage, TResponse> next, CancellationToken cancellationToken)
    {
        // Если для конкретной команды нет валидаторов, нет смысла создавать ValidationContext и запускать validation pipeline.
        if (!_validators.Any())
            return await next(message, cancellationToken);

        // Создаём контекст FluentValidation для текущей команды.
        var context = new ValidationContext<TMessage>(message);

        // Запускаем все зарегистрированные валидаторы параллельно. Это позволяет собрать ошибки от нескольких валидаторов сразу.
        var results = await Task.WhenAll(
            _validators.Select(x => x.ValidateAsync(context, cancellationToken)));

        // Объединяем ошибки всех валидаторов в один массив.
        var failures = results.SelectMany(x => x.Errors).Where(x => x is not null).ToArray();

        // Если хотя бы одна ошибка обнаружена, Handler не вызывается — pipeline прерывается.
        if (failures.Length > 0)
            throw new ValidationException(failures);

        // Validation успешно пройдена.
        // Передаём команду дальше по pipeline. 
        // В конечном итоге здесь будет вызван Handler.
        return await next(message, cancellationToken);
    }
}