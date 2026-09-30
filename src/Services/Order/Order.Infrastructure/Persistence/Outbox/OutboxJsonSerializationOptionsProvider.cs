using System.Text.Json;

namespace Order.Infrastructure.Persistence.Outbox;

/// <summary>
/// Класс инкапсулирует настройку JsonSerializerOptions для сериаолизации и десериализации Outbox.
/// </summary>
public static class OutboxJsonSerializationOptionsProvider
{
    private static readonly JsonSerializerOptions _options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        // future-proofing: можно добавить другие настройки, если понадобятся
    };

    public static JsonSerializerOptions Options => _options;
}
