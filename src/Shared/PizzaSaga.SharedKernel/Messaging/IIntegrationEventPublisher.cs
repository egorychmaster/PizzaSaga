namespace PizzaSaga.SharedKernel.Messaging;

/// <summary>
/// Абстракция публикации интеграционных событий.
/// Application-слой не знает о конкретном messaging framework (MassTransit, Wolverine и т.д.).
/// </summary>
public interface IIntegrationEventPublisher
{
    /// <summary>
    /// Опубликовать интеграционное событие.
    /// </summary>
    /// <param name="integrationEvent">Интеграционное событие.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Task.</returns>
    Task Publish<T>(T integrationEvent, CancellationToken cancellationToken) where T : class;
}
