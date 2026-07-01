using PizzaSaga.ServiceDefaults.Extensions;
using Serilog;

Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Подключаем автоматический OpenTelemetry, логирование и метрики Aspire
    builder.AddServiceDefaults();

    // ... твои стандартные сервисы ...

    var app = builder.Build();

    // Настраиваем эндпоинты для проверки работоспособности (Health Checks)
    app.MapDefaultEndpoints();

    app.Run();

}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}