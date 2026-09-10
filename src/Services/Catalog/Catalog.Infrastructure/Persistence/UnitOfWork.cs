using Catalog.Application.Abstractions.Persistence;
using Catalog.Domain.AggregatesModel.Products.Events;
using Catalog.Infrastructure.Persistence.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PizzaSaga.Contracts.IntegrationEvents.Catalogs;
using PizzaSaga.SharedKernel.Domain;
using System.Text.Json;

namespace Catalog.Infrastructure.Persistence;

/// <summary>
/// Реализация IUnitOfWork для EF Core + PostgreSQL.
/// </summary>
public sealed class UnitOfWork : IUnitOfWork
{    
    private readonly CatalogDbContext _context;
    private readonly ILogger<UnitOfWork> _logger;

    public UnitOfWork(CatalogDbContext context, ILogger<UnitOfWork> logger)
    {        
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => await _context.SaveChangesAsync(cancellationToken);

    public async Task SaveWithOutboxAsync<TAggregate>(TAggregate aggregate, CancellationToken cancellationToken = default)
        where TAggregate : AggregateRootWithId
    {
        ArgumentNullException.ThrowIfNull(aggregate);
        _logger.LogTrace("Saving aggregate {AggregateId} with Outbox.", aggregate.Id);

        // 1. Добавляем агрегат в контекст, если его там нет
        if (_context.Entry(aggregate).State == EntityState.Detached)
            _context.Entry(aggregate).State = EntityState.Added;

        // 2. Сохраняем агрегат
        await _context.SaveChangesAsync(cancellationToken);

        // 3. Извлекаем доменные события
        var domainEvents = aggregate.DomainEvents.ToList();
        if (domainEvents.Count == 0)
        {
            _logger.LogTrace("No domain events for aggregate {AggregateId}.", aggregate.Id);
            return;
        }

        // 4. Создаём Outbox-сообщения и сохраняем их в одной транзакции
        foreach (var domainEvent in domainEvents)
        {
            if (domainEvent is ProductCreatedDomainEvent pce)
            {
                var integrationEvent = new ProductCreatedIntegrationEvent(
                    ProductId: pce.ProductId,
                    Name: pce.Name,
                    Description: pce.Description,
                    PriceAmount: pce.PriceAmount,
                    CurrencyCode: pce.CurrencyCode);

                // ✅ Сохраняем ИНТЕГРАЦИОННОЕ событие в Outbox!
                var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
                var payload = JsonSerializer.Serialize(
                    integrationEvent, 
                    integrationEvent.GetType(),
                    jsonOptions);
                
                _context.OutboxMessages.Add(new OutboxMessage(
                    aggregateId: aggregate.Id,
                    messageType: $"{integrationEvent.GetType().FullName}, {integrationEvent.GetType().Assembly.GetName().Name}",
                    payload: payload));
            }
        }

        // 5. Сохраняем Outbox-сообщения (в той же транзакции)
        await _context.SaveChangesAsync(cancellationToken);

        // 6. Очищаем доменные события
        aggregate.ClearDomainEvents();

        _logger.LogTrace("Outbox saved for aggregate {AggregateId} with {EventCount} events.",
            aggregate.Id, domainEvents.Count);
    }
}