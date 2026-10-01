using MassTransit;

namespace Order.Infrastructure.MassTransit.Saga;

/// <summary>
/// Состояние экземпляра Saga процесса оформления заказа.
/// Не является частью доменного агрегата Order — это persistence model для MassTransit State Machine.
/// </summary>
public sealed class OrderSagaStateData : SagaStateMachineInstance
{
    /// <summary>
    /// Идентификатор экземпляра Saga (PK). Совпадает с OrderId.
    /// Для Order Saga совпадает с идентификатором заказа.
    /// </summary>
    public Guid CorrelationId { get; set; }

    /// <summary>
    /// Текущее техническое состояние Saga.
    /// </summary>
    public string CurrentState { get; set; } = null!;

    /// <summary>
    /// Идентификатор заказа.
    /// </summary>
    public Guid OrderId { get; set; }

    /// <summary>
    /// Идентификатор клиента.
    /// </summary>
    public Guid CustomerId { get; set; }

    /// <summary>
    /// Зафиксированная сумма заказа.
    /// </summary>
    public decimal TotalAmount { get; set; }

    /// <summary>
    /// Код валюты заказа.
    /// </summary>
    public string CurrencyCode { get; set; } = null!;

    /// <summary>
    /// Версия строки Saga для оптимистической блокировки.
    /// В PostgreSQL маппится на системный столбец xmin.
    /// </summary>
    public uint Version { get; set; }
}
