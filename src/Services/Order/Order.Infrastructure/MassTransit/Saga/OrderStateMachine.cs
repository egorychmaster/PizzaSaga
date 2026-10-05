using MassTransit;
using PizzaSaga.Contracts.Orders.IntegrationEvents;
using PizzaSaga.Contracts.Payment.IntegrationCommands;
using PizzaSaga.Contracts.Stock.IntegrationCommands;
using PizzaSaga.Contracts.Stock.IntegrationEvents;

namespace Order.Infrastructure.MassTransit.Saga;

/// <summary>
/// Машина состояний Saga процесса оформления заказа.
/// Оркестрирует резервирование товара, авторизацию оплаты и компенсацию при ошибке авторизации оплаты.
/// </summary>
public sealed class OrderStateMachine : MassTransitStateMachine<OrderSagaStateData>
{
    // --- Состояния ---

    /// <summary>
    /// Состояние: ожидание резервирования товара в Stock Service.
    /// </summary>
    public State AwaitingInventoryReservation { get; private set; } = null!;

    /// <summary>
    /// Состояние: ожидание авторизации оплаты в Payment Service.
    /// </summary>
    public State AwaitingPaymentAuthorization { get; private set; } = null!;

    /// <summary>
    /// Состояние: ожидание освобождения резерва товара (компенсация после ошибки оплаты).
    /// </summary>
    public State AwaitingInventoryRelease { get; private set; } = null!;

    /// <summary>
    /// Терминальное состояние: заказ успешно завершён.
    /// </summary>
    public State Completed { get; private set; } = null!;

    /// <summary>
    /// Терминальное состояние: заказ отменён после компенсации.
    /// </summary>
    public State Cancelled { get; private set; } = null!;


    // --- События (Events) ---

    /// <summary>
    /// Событие создания заказа из Order Service.
    /// </summary>
    public Event<OrderCreatedIntegrationEvent> OrderCreated { get; private set; } = null!;

    /// <summary>
    /// Событие успешного резервирования товара от Stock Service.
    /// </summary>
    public Event<InventoryReservedIntegrationEvent> InventoryReserved { get; private set; } = null!;

    /// <summary>
    /// Событие ошибки резервирования товара от Stock Service.
    /// </summary>
    public Event<InventoryReservationFailedIntegrationEvent> InventoryReservationFailed { get; private set; } = null!;

    /// <summary>
    /// Событие успешной авторизации оплаты от Payment Service.
    /// </summary>
    public Event<PaymentAuthorizedIntegrationEvent> PaymentAuthorized { get; private set; } = null!;

    /// <summary>
    /// Событие ошибки авторизации оплаты от Payment Service.
    /// </summary>
    public Event<PaymentAuthorizationFailedIntegrationEvent> PaymentAuthorizationFailed { get; private set; } = null!;

    /// <summary>
    /// Событие успешного освобождения резерва товара (компенсация) от Stock Service.
    /// </summary>
    public Event<InventoryReleasedIntegrationEvent> InventoryReleased { get; private set; } = null!;

    /// <summary>
    /// Конструктор: определяет граф переходов State Machine.
    /// </summary>
    public OrderStateMachine()
    {
        InstanceState(x => x.CurrentState);

        // Корреляция всех событий по OrderId
        // 1. Стартовое событие: ищем существующий ИЛИ задаем CorrelationId для нового инстанса
        Event(() => OrderCreated, eventConfigurator =>
        {
            // 1. Ищем существующий. Проверяет БД — не находит (это нормально).
            eventConfigurator.CorrelateById(context => context.Message.OrderId);
            // 2. Создаём новый. Если нет, создай новый экземпляр с CorrelationId = OrderId
            eventConfigurator.SelectId(context => context.Message.OrderId);
        });
        // 2. Последующие события: ТОЛЬКО поиск существующего инстанса по OrderId
        Event(() => InventoryReserved, eventConfigurator =>
        {
            eventConfigurator.CorrelateById(context => context.Message.OrderId);
        });

        Event(() => InventoryReservationFailed, eventConfigurator =>
        {
            eventConfigurator.CorrelateById(context => context.Message.OrderId);
        });

        Event(() => PaymentAuthorized, eventConfigurator =>
        {
            eventConfigurator.CorrelateById(context => context.Message.OrderId);
        });

        Event(() => PaymentAuthorizationFailed, eventConfigurator =>
        {
            eventConfigurator.CorrelateById(context => context.Message.OrderId);
        });

        Event(() => InventoryReleased, eventConfigurator =>
        {
            eventConfigurator.CorrelateById(context => context.Message.OrderId);
        });


        // 1. Старт Саги при создании заказа
        Initially(
            When(OrderCreated)
                // синхронная инициализация данных
                .Then(context =>
                {
                    context.Saga.OrderId = context.Message.OrderId;
                    context.Saga.CustomerId = context.Message.CustomerId;
                    context.Saga.TotalAmount = context.Message.TotalAmount;
                    context.Saga.CurrencyCode = context.Message.CurrencyCode;
                })
                // Отправляем команду Stock Service для резервирования товара.
                .Send(context =>
                        new ReserveInventoryIntegrationCommand(
                            context.Message.OrderId,
                            // OrderCreatedIntegrationEvent содержит детализацию по позициям
                            context.Message.Items.Select(item =>
                                new ProductQuantity(item.ProductId, item.Quantity))))
                // переход в новое состояние
                .TransitionTo(AwaitingInventoryReservation)
                );

        // 2. Ожидание резервирования товара от Stock Service
        During(
            AwaitingInventoryReservation,
            // Сценарий 1: Резерв успешен -> Запрашиваем авторизацию платежа
            When(InventoryReserved)
                .Send(context =>
                        new AuthorizePaymentIntegrationCommand(
                            context.Message.OrderId,
                            context.Saga.TotalAmount,
                            context.Saga.CurrencyCode))
                .TransitionTo(AwaitingPaymentAuthorization),
            // Сценарий 2: Резерв отклонен -> Отменяем заказ
            When(InventoryReservationFailed)
                .Publish(context =>
                        new OrderCancelledIntegrationEvent(
                            context.Message.OrderId,
                            context.Saga.CustomerId))
                .TransitionTo(Cancelled)
                );

        // 3. Ожидание авторизации оплаты
        During(AwaitingPaymentAuthorization,
            When(PaymentAuthorized)
                .Publish(context =>
                        new OrderCompletedIntegrationEvent(
                            context.Message.OrderId,
                            context.Saga.CustomerId,
                            context.Saga.TotalAmount,
                            context.Saga.CurrencyCode))
                .TransitionTo(Completed),

            When(PaymentAuthorizationFailed)
                .Send(context =>
                    new ReleaseInventoryIntegrationCommand(
                        context.Message.OrderId))
                .TransitionTo(AwaitingInventoryRelease));

        // --- Ожидание освобождения резерва (компенсация) ---
        During(
            AwaitingInventoryRelease,
            When(InventoryReleased)
                .Publish(context =>
                        new OrderCancelledIntegrationEvent(
                            context.Message.OrderId,
                            context.Saga.CustomerId))
                .TransitionTo(Cancelled));

        // Конфигурация терминальных состояний
        SetCompletedWhenFinalized();
    }
}
