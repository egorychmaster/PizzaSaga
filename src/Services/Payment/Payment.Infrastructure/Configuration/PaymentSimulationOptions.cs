namespace Payment.Infrastructure.Configuration;

/// <summary>
/// Настройки симуляции банковской авторизации платежей.
/// </summary>
public sealed class PaymentSimulationOptions
{
    public const string SectionName = "PaymentSimulation";

    /// <summary>
    /// Если true — имитировать отказ банка при авторизации.
    /// </summary>
    public bool SimulateFailure { get; set; }
}
