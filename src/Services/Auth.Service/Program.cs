var builder = WebApplication.CreateBuilder(args);

// Подключаем автоматический OpenTelemetry, логирование и метрики Aspire
builder.AddServiceDefaults();

// ... твои стандартные сервисы ...

var app = builder.Build();

// Настраиваем эндпоинты для проверки работоспособности (Health Checks)
app.MapDefaultEndpoints();

app.Run();

