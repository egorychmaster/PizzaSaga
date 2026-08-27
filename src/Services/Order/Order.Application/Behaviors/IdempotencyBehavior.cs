using Mediator;
using Microsoft.Extensions.Logging;
using Order.Application.Abstractions.Persistence;
using Order.Application.Abstractions.Persistence.Idempotency;
using Order.Application.Abstractions.Persistence.Idempotency.Exceptions;
using System.Text.Json;

namespace Order.Application.Behaviors;

/// <summary>
/// Pipeline Behavior для обеспечения HTTP idempotency команд.
/// Выполняет replay сохранённого результата и защищает от конкурентной обработки одного Idempotency-Key.
/// </summary>
public sealed class IdempotencyBehavior<TMessage, TResponse>
    : IPipelineBehavior<TMessage, TResponse>
    where TMessage : ICommand<TResponse>
{
    private readonly IIdempotencyContext _idempotencyContext;
    private readonly IIdempotencyRepository _idempotencyRepository;
    private readonly ITransactionCallbackContext _transactionCallbackContext;
    private readonly ILogger<IdempotencyBehavior<TMessage, TResponse>> _logger;

    public IdempotencyBehavior(
        IIdempotencyContext idempotencyContext,
        IIdempotencyRepository idempotencyRepository,
        ITransactionCallbackContext transactionCallbackContext,
        ILogger<IdempotencyBehavior<TMessage, TResponse>> logger)
    {
        _idempotencyContext = idempotencyContext ?? throw new ArgumentNullException(nameof(idempotencyContext));
        _idempotencyRepository = idempotencyRepository  ?? throw new ArgumentNullException(nameof(idempotencyRepository));
        _transactionCallbackContext = transactionCallbackContext ?? throw new ArgumentNullException(nameof(transactionCallbackContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async ValueTask<TResponse> Handle(TMessage message, MessageHandlerDelegate<TMessage, TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_idempotencyContext.IsSet)
        {
            return await next(message, cancellationToken);
        }

        var key = _idempotencyContext.Key;
        var requestHash = _idempotencyContext.RequestHash!;

        try
        {
            // Шаг 1: Проверяем наличие сохранённого результата (Fast Path)
            var existingRecord = await _idempotencyRepository.GetAsync(key, cancellationToken);
            if (existingRecord is not null)
            {
                return HandleExistingRecord<TResponse>(existingRecord, requestHash);
            }

            // Шаг 2: Выполняем handler и сохраняем результат

            // Регистрируем callback для сохранения IdempotencyRecord внутри транзакции TransactionBehavior.
            _transactionCallbackContext.Register(
                async (result, ct) =>
                {
                    var typedResult = (TResponse)result!;
                    var responseBody = JsonSerializer.Serialize(typedResult);

                    var record = new IdempotencyRecordDto(
                        IdempotencyKey: key,
                        RequestHash: requestHash,
                        ResponseStatusCode: _idempotencyContext.ResponseStatusCode,
                        ResponseBody: responseBody,
                        CreatedAt: DateTimeOffset.UtcNow);

                    await _idempotencyRepository.AddAsync(record, ct);
                });

            return await next(message, cancellationToken);
        }
        catch (DuplicateIdempotencyKeyException ex)
        {
            _logger.LogInformation(
                ex,
                "Concurrent request detected for Idempotency-Key {IdempotencyKey}. " +
                "Transaction was rolled back; attempting to replay the result.",
                key);

            var existingRecord = await _idempotencyRepository.GetAsync(key, cancellationToken);
            if (existingRecord is null)
            {
                // Первая транзакция ещё не успела закоммитить запись.
                throw new IdempotencyConflictException("The request with the specified Idempotency-Key is currently being processed.");
            }

            return HandleExistingRecord<TResponse>(existingRecord, requestHash);
        }
    }

    /// <summary>
    /// Обрабатывает уже существующую запись идемпотентности.
    /// </summary>
    private static TResponse HandleExistingRecord<TResponse>(IdempotencyRecordDto record, string requestHash)
    {
        if (!string.Equals(record.RequestHash, requestHash, StringComparison.OrdinalIgnoreCase))
        {
            throw new IdempotencyConflictException("The specified Idempotency-Key has already been used with a different request body.");
        }

        return JsonSerializer.Deserialize<TResponse>(record.ResponseBody)
               ?? throw new InvalidOperationException("The stored idempotency response could not be deserialized.");
    }
}