using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using Payment.Domain.AggregatesModel.PaymentReservation;
using Payment.Infrastructure.PaymentGateways;
using PizzaSaga.Contracts.Payment.IntegrationCommands;
using PizzaSaga.Contracts.Payment.IntegrationEvents;

namespace Payment.Infrastructure.MassTransit.CommandConsumers;

/// <summary>
/// Консьюмер команды AuthorizePaymentIntegrationCommand.
/// Обеспечивает бизнес-идемпотентность на уровне платёжной резервации.
/// </summary>
public class AuthorizePaymentConsumer : IConsumer<AuthorizePaymentIntegrationCommand>
{
    private readonly IPaymentReservationRepository _reservationRepo;
    private readonly IPaymentGateway _paymentGateway;
    private readonly ILogger<AuthorizePaymentConsumer> _logger;

    public AuthorizePaymentConsumer(
        IPaymentReservationRepository reservationRepo,
        IPaymentGateway paymentGateway,
        ILogger<AuthorizePaymentConsumer> logger)
    {
        _reservationRepo = reservationRepo ?? throw new ArgumentNullException(nameof(reservationRepo));
        _paymentGateway = paymentGateway ?? throw new ArgumentNullException(nameof(paymentGateway));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task Consume(ConsumeContext<AuthorizePaymentIntegrationCommand> context)
    {
        var message = context.Message;

        // 1. Проверка идемпотентности: платёжная резервация уже существует?
        var existingReservation = await _reservationRepo.FindByOrderIdAsync(message.OrderId, context.CancellationToken);
        if (existingReservation != null)
        {
            // 3. Проверка неизменяемости параметров — сумма и валюта должны совпадать
            if (!existingReservation.TotalAmount.Amount.Equals(message.Amount) ||
                !existingReservation.TotalAmount.Currency.Code.Equals(message.CurrencyCode))
            {
                _logger.LogWarning(
                    "Payment parameters mismatch for OrderId={OrderId}. " +
                    "Existing: Amount={ExistingAmount}, Currency={ExistingCurrency}; " +
                    "Requested: Amount={RequestedAmount}, Currency={RequestedCurrency}",
                    message.OrderId,
                    existingReservation.TotalAmount.Amount,
                    existingReservation.TotalAmount.Currency.Code,
                    message.Amount,
                    message.CurrencyCode);

                // Публикуем отказ с причиной
                await context.Publish(new PaymentAuthorizationFailedIntegrationEvent(
                    message.OrderId,
                    message.Amount,
                    message.CurrencyCode,
                    "Payment parameters mismatch: existing reservation has different amount/currency"));

                return;
            }

            // 2. Терминальные состояния — повторная обработка не требуется
            if (existingReservation.IsTerminal())
            {
                _logger.LogInformation(
                    "Payment for OrderId={OrderId} already processed with status={Status}. Idempotent no-op.",
                    message.OrderId,
                    existingReservation.Status);

                // Воспроизвести результат на основе сохранённого статуса
                switch (existingReservation.Status)
                {
                    case PaymentStatus.Authorized:
                        await context.Publish(new PaymentAuthorizedIntegrationEvent(message.OrderId));
                        break;

                    case PaymentStatus.Failed:
                        await context.Publish(new PaymentAuthorizationFailedIntegrationEvent(
                            message.OrderId,
                            existingReservation.TotalAmount.Amount,
                            existingReservation.TotalAmount.Currency.Code,
                            existingReservation.FailureReason ?? $"Payment was previously failed: {existingReservation.Status}"));
                        break;

                    case PaymentStatus.Cancelled:
                        // Согласно бизнес-контракту: отмена означает невозможность повторной авторизации
                        await context.Publish(new PaymentAuthorizationFailedIntegrationEvent(
                            message.OrderId,
                            existingReservation.TotalAmount.Amount,
                            existingReservation.TotalAmount.Currency.Code,
                            existingReservation.FailureReason ?? "Payment was cancelled and cannot be re-authorized"));
                        break;
                }

                return;
            }

            // Если статус Pending — операция не завершена, продолжаем обработку
        }

        _logger.LogInformation(
            "Processing AuthorizePayment for OrderId={OrderId}, Amount={Amount}, Currency={Currency}",
            message.OrderId,
            message.Amount,
            message.CurrencyCode);

        // 4. Создать или получить резервацию и обновить статус
        var paymentReservation = existingReservation ?? PaymentReservationAggregate.Create(
            message.OrderId,
            message.Amount,
            message.CurrencyCode);

        // 5. Интеграция с банковским шлюзом (с симуляцией банка).
        var gatewayResult = await _paymentGateway.AuthorizeAsync(
            message.OrderId,
            message.Amount,
            message.CurrencyCode,
            context.CancellationToken);
        if (!gatewayResult.IsApproved)
        {
            paymentReservation.Fail(gatewayResult.DeclineReason ?? "Payment declined by bank");
        }
        else
        {
            paymentReservation.Authorize();
        }

        if (existingReservation is null)
        {
            await _reservationRepo.AddAsync(paymentReservation, context.CancellationToken);
        }
        else
        {
            // EF Core сам отслеживает изменения агрегата — обновлять не нужно
        }

        try
        {
            await _reservationRepo.SaveChangesAsync(context.CancellationToken);

            _logger.LogInformation("Payment successfully authorized for OrderId={OrderId}", message.OrderId);

            // 5. Публикация результата через Outbox (в рамках той же транзакции)
            await context.Publish(new PaymentAuthorizedIntegrationEvent(message.OrderId));
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogError(ex, "Optimistic concurrency conflict while authorizing payment for OrderId={OrderId}", message.OrderId);

            // Транзакция будет откачена автоматически — передаём MassTransit для retry
            throw;
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: "23505" })
        {
            _logger.LogInformation(ex, "Duplicate payment reservation for OrderId={OrderId}. Idempotent no-op, retry will find existing record.", message.OrderId);

            // Транзакция будет откачена. При retry новая транзакция найдёт существующую запись и обработает как no-op.
            throw;
        }
    }
}
