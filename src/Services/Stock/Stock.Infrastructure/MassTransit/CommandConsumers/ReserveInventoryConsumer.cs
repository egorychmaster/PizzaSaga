using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PizzaSaga.Contracts.Stock.IntegrationCommands;
using PizzaSaga.Contracts.Stock.IntegrationEvents;
using Stock.Domain.AggregatesModel.Inventory;
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
        if (existingReservation is { Status: ReservationStatus.Reserved or ReservationStatus.Released })
        {
            _logger.LogInformation("A reservation for OrderId={OrderId} already exists. Idempotent no-op.", message.OrderId);

            // Повторно опубликовать событие
            await context.Publish(new InventoryReservedIntegrationEvent(message.OrderId));
            return;
        }


        // 2. Получить все ProductId для продуктов и проверить доступность ДО транзакции
        var items = message.Products.Select(p => new InventoryReservationItem(p.ProductId, p.Quantity)).ToList();
        var inventoryIds = items.Select(i => i.ProductId).Distinct().ToArray();

        var inventories = await _inventoryRepo.GetByIdsAsync(inventoryIds, context.CancellationToken);

        // Проверить доступность ТЕПЕРЬ, до начала транзакции
        foreach (var item in items)
        {
            var inventory = inventories.FirstOrDefault(i => i.ProductId == item.ProductId);
            if (inventory is null || !inventory.CanReserve(item.Quantity))
            {
                _logger.LogWarning(
                    "Insufficient stock for OrderId={OrderId}, ProductId={ProductId}. Available: {Available}, требуется: {Required}",
                    message.OrderId,
                    item.ProductId,
                    inventory?.AvailableQuantity ?? 0,
                    item.Quantity
                );

                // Публикуем событие об отказе без сохранения резерва
                await context.Publish(new InventoryReservationFailedIntegrationEvent(
                    message.OrderId,
                    item.ProductId,
                    item.Quantity,
                    inventory?.AvailableQuantity ?? 0,
                    inventory is null
                        ? $"Product with Id={item.ProductId} not found"
                        : $"Insufficient stock. Available: {inventory.AvailableQuantity}, required: {item.Quantity}"
                ));

                // Завершаем consumer без сохранения изменений
                return;
            }
        }

        // 3. Зарезервировать остатки
        // Автоматическое управление транзакцией: MassTransit сам открывает транзакцию БД перед вызовом метода Consume,
        // выполняет ваш код и автоматически делает CommitAsync() (если не было исключений) или RollbackAsync() (если вылетело исключение).
        try
        {            
            // 3.1 Бизнес-логика (проверка доступности и резервирование)
            foreach (var item in items)
            {
                var inventory = inventories.FirstOrDefault(i => i.ProductId == item.ProductId);
                // Исключение, если товар из существующего резерва отсутствует в базе. Молча пропускать его опасно.
                if (inventory is null)
                    throw new InvalidOperationException($"Inventory not found for ProductId={item.ProductId}.");

                inventory.Reserve(item.Quantity);
            }

            // 3.2 Создание записи резерва
            var reservation = InventoryReservationAggregate.Create(message.OrderId, items);
            await _reservationRepo.AddAsync(reservation, context.CancellationToken);

            // 3.3 Единый вызов SaveChangesAsync. 
            // MassTransit перехватит этот момент, запишет исходящие сообщения в таблицу OutboxMessage в рамках той же транзакции и закоммитит всё атомарно.
            await _dbContext.SaveChangesAsync(context.CancellationToken);

            _logger.LogInformation("The reserve has been successfully created for OrderId={OrderId}", message.OrderId);

            // 3.4 Публикация успешного события
            await context.Publish(new InventoryReservedIntegrationEvent(message.OrderId));
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogError(ex, "Concurrency conflict while releasing inventory for OrderId={OrderId}", message.OrderId);

            // Откат транзакции произойдет автоматически при Dispose
            throw; // Передать MassTransit для retry
        }
    }
}
