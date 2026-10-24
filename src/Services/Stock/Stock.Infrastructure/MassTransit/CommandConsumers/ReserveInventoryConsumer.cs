using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using PizzaSaga.Contracts.Stock.IntegrationCommands;
using PizzaSaga.Contracts.Stock.IntegrationEvents;
using Stock.Domain.AggregatesModel.Inventory;
using Stock.Domain.AggregatesModel.Inventory.Exceptions;
using Stock.Domain.AggregatesModel.InventoryReservation;
using Stock.Infrastructure.Persistence;

namespace Stock.Infrastructure.MassTransit.CommandConsumers;

/// <summary>
/// Consumer для обработки команды резервирования инвентаря.
/// Реализует бизнес-идемпотентность по OrderId и транзакционное сохранение остатков и резерва.
/// </summary>
public sealed class ReserveInventoryConsumer : IConsumer<ReserveInventoryIntegrationCommand>
{
    private readonly StockDbContext _dbContext;
    private readonly ILogger<ReserveInventoryConsumer> _logger;
    private readonly IInventoryReservationRepository _reservationRepo;
    private readonly IInventoryRepository _inventoryRepo;

    /// <summary>
    /// Инициализирует новый экземпляр consumer'а.
    /// </summary>
    public ReserveInventoryConsumer(
        StockDbContext dbContext,
        ILogger<ReserveInventoryConsumer> logger,
        IInventoryReservationRepository reservationRepo,
        IInventoryRepository inventoryRepo)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _reservationRepo = reservationRepo ?? throw new ArgumentNullException(nameof(reservationRepo));
        _inventoryRepo = inventoryRepo ?? throw new ArgumentNullException(nameof(inventoryRepo));
    }

    /// <inheritdoc />
    public async Task Consume(ConsumeContext<ReserveInventoryIntegrationCommand> context)
    {
        // В документации и реализации MassTransit EF Consumer Outbox транзакция открывается на время обработки сообщения,
        // а изменения и исходящие сообщения сохраняются до её коммита. При ошибке транзакция откатывается.

        var message = context.Message;

        // 1. Проверка идемпотентности: резерв уже существует?
        var existingReservation = await _reservationRepo.FindByOrderIdAsync(message.OrderId, context.CancellationToken);
        if (existingReservation is not null)
        {
            if (existingReservation.Status == ReservationStatus.Reserved)
            {
                _logger.LogInformation("Reservation for OrderId={OrderId} already exists. Idempotent no-op.", message.OrderId);
                await context.Publish(new InventoryReservedIntegrationEvent(message.OrderId));
            }
            else // Released
            {
                _logger.LogWarning("Reservation for OrderId={OrderId} was already released. Reserve command ignored.", message.OrderId);
                await context.Publish(new InventoryReservationFailedIntegrationEvent(
                    OrderId: message.OrderId, Reason: "Reservation was already released"));
            }
            return;
        }


        // 2. Зарезервировать остатки
        // Автоматическое управление транзакцией: MassTransit сам открывает транзакцию БД перед вызовом метода Consume,
        // выполняет ваш код и автоматически делает CommitAsync() (если не было исключений) или RollbackAsync() (если вылетело исключение).
        try
        {
            var items = message.Products.Select(p => new InventoryReservationItem(p.ProductId, p.Quantity)).ToList();
            var inventoryIds = items.Select(i => i.ProductId).Distinct().ToArray();
            var inventories = await _inventoryRepo.GetByIdsAsync(inventoryIds, context.CancellationToken);

            // 2.1 Бизнес-логика (проверка доступности и резервирование)
            foreach (var item in items)
            {
                var inventory = inventories.FirstOrDefault(i => i.ProductId == item.ProductId);
                if (inventory is null)
                {
                    _logger.LogWarning("Product with Id={ProductId} not found in inventory.", item.ProductId);
                    await context.Publish(new InventoryReservationFailedIntegrationEvent(
                        OrderId: message.OrderId,
                        Reason: $"Product with Id={item.ProductId} not found",
                        ProductId: item.ProductId,
                        RequestedQuantity: item.Quantity));

                    return;
                }

                inventory.Reserve(item.Quantity);
            }

            // 2.2 Создание записи резерва
            var reservation = InventoryReservationAggregate.Create(message.OrderId, items);
            await _reservationRepo.AddAsync(reservation, context.CancellationToken);

            // 2.3 Единый вызов SaveChangesAsync. 
            // MassTransit перехватит этот момент, запишет исходящие сообщения в таблицу OutboxMessage в рамках той же транзакции и закоммитит всё атомарно.
            await _dbContext.SaveChangesAsync(context.CancellationToken);

            _logger.LogInformation("The reserve has been successfully created for OrderId={OrderId}", message.OrderId);

            // 2.4 Публикация успешного события
            await context.Publish(new InventoryReservedIntegrationEvent(message.OrderId));
        }
        catch (InsufficientAvailableQuantityException ex)
        {
            _logger.LogInformation(
                ex,
                "Business validation failed while reserving inventory for OrderId={OrderId}, ProductId={ProductId}",
                message.OrderId,
                ex.ProductId);

            await context.Publish(new InventoryReservationFailedIntegrationEvent(
                OrderId: message.OrderId,
                Reason: ex.Message,
                ProductId: ex.ProductId,
                RequestedQuantity: ex.RequestedQuantity,
                AvailableQuantity: ex.AvailableQuantity));

            // Не пробрасываем исключение — это ожидаемая бизнес-ошибка, retry не нужен
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogError(ex, "Optimistic concurrency conflict while reserving inventory for OrderId={OrderId}", message.OrderId);

            // Откат транзакции произойдет автоматически при Dispose
            throw; // Передать MassTransit для retry
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: "23505" })
        {
            _logger.LogInformation(
                ex,
                "Duplicate inventory reservation for OrderId={OrderId}. Idempotent no-op, retry will find existing record.",
                message.OrderId);

            // Транзакция будет откачена. При retry новая транзакция найдёт существующую запись и обработает как no-op.
            throw;
        }
    }
}
