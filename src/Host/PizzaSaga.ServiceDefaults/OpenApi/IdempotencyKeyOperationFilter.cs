using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace PizzaSaga.ServiceDefaults.OpenApi;

/// <summary>
/// Добавляет обязательный заголовок Idempotency-Key в описание операции создания заказа.
/// </summary>
public sealed class IdempotencyKeyOperationFilter : IOperationFilter
{
    /// <summary>
    /// Дополняет OpenAPI-описание операции параметром Idempotency-Key.
    /// </summary>
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        // Фильтр применяется только к endpoint'у с OperationId CreateOrder.
        if (operation.OperationId != "CreateOrder")
            return;

        operation.Parameters ??= [];

        // Не допускаем повторного добавления параметра.
        if (operation.Parameters.Any(parameter =>
                parameter.Name == "Idempotency-Key" &&
                parameter.In == ParameterLocation.Header))
        {
            return;
        }

        operation.Parameters.Add(new OpenApiParameter
        {
            Name = "Idempotency-Key",
            In = ParameterLocation.Header,
            Description = "Unique identifier for ensuring idempotent order creation.",
            Required = true,
            Schema = new OpenApiSchema
            {
                Type = JsonSchemaType.String,
                Format = "uuid"
            }
        });
    }
}