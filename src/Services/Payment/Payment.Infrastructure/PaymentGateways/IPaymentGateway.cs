namespace Payment.Infrastructure.PaymentGateways;

/// <summary>
/// Интерфейс банковского шлюза для авторизации платежей.
/// 
/// Отвечает за взаимодействие с внешним эквайрингом или его симуляцией.
/// Реализации должны быть без побочных эффектов (чистые) и асинхронными.
/// </summary>
public interface IPaymentGateway
{
    /// <summary>
    /// Имитирует запрос к банку на авторизацию платежа.
    /// 
    /// В реальной реализации здесь был бы вызов HTTP-сервиса банка (например, через Stripe/Tinkoff/Alpha).
    /// В тестовой/отладочной среде — симуляция через FakePaymentGateway.
    /// </summary>
    /// <param name="orderId">Идентификатор заказа в системе.</param>
    /// <param name="amount">Сумма платежа (в основной валюте, без копеек).</param>
    /// <param name="currency">Код валюты в ISO 4217 (например, "RUB", "USD").</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>
    /// Результат авторизации:
    /// - IsApproved = true → можно переходить к резервированию средств.
    /// - IsApproved = false → необходимо проинформировать пользователя и сохранить причину отказа.
    /// </returns>
    Task<PaymentGatewayResult> AuthorizeAsync(
        Guid orderId,
        decimal amount,
        string currency,
        CancellationToken cancellationToken);
}

/// <summary>
/// Результат авторизации платежа банковским шлюзом.
/// 
/// Используется как неизменяемый (immutable) результат операции:
/// - IsApproved == true  → банк одобрил платёж.
/// - IsApproved == false → банк отклонил платёж с причиной (DeclineReason).
/// </summary>
/// <param name="IsApproved">Признак успешной авторизации: true — одобрено, false — отклонено. </param>
/// <param name="DeclineReason">Причина отказа. Обязательна только при IsApproved == false. По умолчанию: null (для успешных результатов) или "Unknown decline reason".</param>
public sealed record PaymentGatewayResult(
    bool IsApproved,
    string? DeclineReason = null)
{
    /// <summary>
    /// Создаёт результат одобрения платежа.
    /// </summary>
    /// <returns>PaymentGatewayResult с IsApproved = true, DeclineReason = null.</returns>
    public static PaymentGatewayResult Approved() => new(true);

    /// <summary>
    /// Создаёт результат отказа в авторизации.
    /// </summary>
    /// <param name="reason">Причина отказа. Если null — используется "Unknown decline reason".</param>
    /// <returns>PaymentGatewayResult с IsApproved = false и указанной причиной.</returns>
    public static PaymentGatewayResult Declined(string? reason) => new(false, reason ?? "Unknown decline reason");
}