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
/// Consumer для обработки команды освобождения инвентаря.
/// Вызывается при отмене заказа или при необходимости вернуть товар на склад.
/// Обеспечивает бизнес-идемпотентность по OrderId.
/// </summary>
public sealed class ReleaseInventoryConsumer : IConsumer<ReleaseInventoryIntegrationCommand>
{
    private readonly StockDbContext _dbContext;
    private readonly ILogger<ReleaseInventoryConsumer> _logger;
    private readonly IInventoryReservationRepository _reservationRepo;
    private readonly IInventoryRepository _inventoryRepo;

    /// <summary>
    /// Инициализирует новый экземпляр consumer'а.
    /// </summary>
    public ReleaseInventoryConsumer(
        StockDbContext dbContext,
        ILogger<ReleaseInventoryConsumer> logger,
        IInventoryReservationRepository reservationRepo,
        IInventoryRepository inventoryRepo)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _reservationRepo = reservationRepo ?? throw new ArgumentNullException(nameof(reservationRepo));
        _inventoryRepo = inventoryRepo ?? throw new ArgumentNullException(nameof(inventoryRepo));
    }

    /// <inheritdoc />
    public async Task Consume(ConsumeContext<ReleaseInventoryIntegrationCommand> context)
    {
        var message = context.Message;

        // 1. Найти резерв по OrderId
        var reservation = await _reservationRepo.FindByOrderIdAsync(message.OrderId, context.CancellationToken);
        if (reservation is null)
        {
            _logger.LogWarning("Reservation for OrderId={OrderId} not found. Compensation skipped.", message.OrderId);
            // Согласно контракту, это допустимый сценарий — можно просто вернуть успех
            await context.Publish(new InventoryReleasedIntegrationEvent(message.OrderId));
            return;
        }

        // 2. Повторное освобождение не должно менять остатки.
        if (reservation.Status == ReservationStatus.Released)
        {
            _logger.LogInformation("The reservation for OrderId={OrderId} has already been released. Idempotent no-op.", message.OrderId);
            await context.Publish(new InventoryReleasedIntegrationEvent(message.OrderId));
            return;
        }


        // 3. Освободить товары и обновить статус в одной транзакции
        try
        {
            // 3.1 Освободить каждый товар.
            foreach (var item in reservation.Items)
            {
                var inventory = await _inventoryRepo.GetByIdAsync(item.ProductId, context.CancellationToken);
                // Исключение, если товар из существующего резерва отсутствует в базе.
                // Молча пропускать его опасно: можно отметить резерв освобождённым, фактически не восстановив все остатки.
                if (inventory is null)
                    throw new InvalidOperationException($"Inventory not found for ProductId={item.ProductId}.");

                inventory.Release(item.Quantity);
            }

            // 3.2 Перевести резерв в конечное состояние.
            reservation.Release();

            // 3.3 Сохранить изменения остатков и резерва.
            // MassTransit перехватит его и закоммитит изменения вместе с Outbox-сообщениями в одной транзакции.
            await _dbContext.SaveChangesAsync(context.CancellationToken);

            // 3.4 Опубликовать событие через Consumer Outbox.
            // Фактическая публикация произойдёт после успешной фиксации транзакции, которой управляет настроенный middleware.
            await context.Publish(new InventoryReleasedIntegrationEvent(message.OrderId));

            _logger.LogInformation("Reservation successfully released for OrderId={OrderId}", message.OrderId);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogError(ex, "Concurrency error when releasing the reserve for OrderId={OrderId}", message.OrderId);

            // Откат транзакции произойдет автоматически при Dispose
            throw; // Retry
        }
    }
}
