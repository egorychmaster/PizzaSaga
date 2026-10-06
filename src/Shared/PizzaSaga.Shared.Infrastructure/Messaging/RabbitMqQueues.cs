namespace PizzaSaga.Shared.Infrastructure.Messaging;

/// <summary>
/// Централизованные имена очередей RabbitMQ для всей системы.
/// </summary>
public static class RabbitMqQueues
{
    // --- Очереди Order Service ---

    /// <summary>
    /// Очередь для хранения состояний Saga заказов (автоматически создается MassTransit).
    /// </summary>
    public const string OrderSaga = "Order-Saga";

    /// <summary>
    /// Очередь для события успешного завершения заказа.
    /// Используется Order-Service как consumer.
    /// </summary>
    public const string OrderCompleted = "Order-OrderCompleted";

    /// <summary>
    /// Очередь для события отмены заказа.
    /// Используется Order-Service как consumer.
    /// </summary>
    public const string OrderCancelled = "Order-OrderCancelled";

    /// <summary>
    /// Очередь для интеграционного события создания продукта.
    /// Используется Order-Service как consumer.
    /// </summary>
    public const string OrderProductCreated = "Order-ProductCreated";


    // --- Очереди Stock Service ---

    /// <summary>
    /// Очередь для команды резервирования инвентаря.
    /// Используется Stock-Service как consumer.
    /// Отправляется Order-Service из OrderStateMachine.
    /// </summary>
    public const string StockReserveInventory = "Stock-ReserveInventory";

    /// <summary>
    /// Очередь для команды освобождения резерва инвентаря (компенсация).
    /// Используется Stock-Service как consumer.
    /// Отправляется Order-Service из OrderStateMachine при ошибке авторизации оплаты.
    /// </summary>
    public const string StockReleaseInventory = "Stock-ReleaseInventory";

    /// <summary>
    /// Очередь для интеграционного события создания продукта.
    /// Используется Stock-Service как consumer.
    /// </summary>
    public const string StockProductCreated = "Stock-ProductCreated";


    // --- Очереди Payment Service ---

    /// <summary>
    /// Очередь для команды авторизации платежа.
    /// Используется Payment-Service как consumer.
    /// Отправляется Order-Service из OrderStateMachine.
    /// </summary>
    public const string PaymentAuthorizePayment = "Payment-AuthorizePayment";

    /// <summary>
    /// Очередь для команды отмены платежа (компенсация).
    /// Используется Payment-Service как consumer.
    /// Отправляется Order-Service из OrderStateMachine при компенсации заказа.
    /// </summary>
    public const string PaymentCancelPayment = "Payment-CancelPayment";

}
