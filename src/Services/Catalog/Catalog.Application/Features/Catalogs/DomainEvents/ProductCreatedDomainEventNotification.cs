using Catalog.Domain.AggregatesModel.Products.Events;
using Mediator;

namespace Catalog.Application.Features.Catalogs.DomainEvents;

/// <summary>
/// Нотификация доменного события создания продукта.
/// Используется для преобразования внутреннего доменного события во внешнее интеграционное событие.
/// </summary>
public sealed record ProductCreatedDomainEventNotification(ProductCreatedDomainEvent DomainEvent) : INotification;
