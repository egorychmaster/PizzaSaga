using PizzaSaga.SharedKernel.Domain;

namespace Catalog.Application.Abstractions.DomainEvents;

/// <summary>
/// Интерфейс диспетчера доменных событий.
/// Передает доменные события зарегистрированным обработчикам внутри текущей транзакции.
/// Отвечает за публикацию доменных событий через Mediator без зависимости Domain от Mediator.
/// </summary>
public interface IDomainEventDispatcher
{
    /// <summary>
    /// Обрабатывает доменные события агрегата.
    /// Публикует коллекцию доменных событий через Mediator.
    /// </summary>
    /// <param name="domainEvents">Коллекция доменных событий.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    Task DispatchAsync(IReadOnlyCollection<IDomainEvent> domainEvents, CancellationToken cancellationToken = default);
}