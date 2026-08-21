namespace Order.Application.Abstractions.Persistence.Idempotency;

/// <summary>
/// Интерфейс абстракции для работы с хранилищем идемпотентности.
/// Не вмешивается в бизнес-логику, предоставляет только операции CRUD.
/// </summary>
public interface IIdempotencyRepository
{
    /// <summary>
    /// Попытаться найти запись по ключу.
    /// Возвращает null, если запись не найдена.
    /// </summary>
    /// <param name="idempotencyKey">Идентификатор идемпотентности (Guid из заголовка).</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Сущность IdempotencyRecordDto или null.</returns>
    Task<IdempotencyRecordDto?> GetAsync(Guid idempotencyKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Добавить новую запись в хранилище.
    /// Вызывается из транзакционной области.
    /// </summary>
    /// <param name="entity">Запись для добавления.</param>
    /// <param name="cancellationToken"></param>
    Task AddAsync(IdempotencyRecordDto record, CancellationToken cancellationToken = default);
}