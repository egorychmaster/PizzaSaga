var builder = WebApplication.CreateBuilder(args);

// Подключаем автоматический OpenTelemetry, логирование и метрики Aspire
builder.AddServiceDefaults();

// ... твои стандартные сервисы ...

var app = builder.Build();

// Настраиваем эндпоинты для проверки работоспособности (Health Checks)
app.MapDefaultEndpoints();

app.MapGet("/api/orders", () => new[]
{
    new { Id = 1, CakeName = "Margarita", Status = "Pending" },
    new { Id = 2, CakeName = "Pepperoni", Status = "Baking" }
});

app.Run();