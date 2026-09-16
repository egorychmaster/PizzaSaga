using PizzaSaga.ServiceDefaults.Extensions;
using PizzaSaga.ServiceDefaults.Extensions.Aspires;
using PizzaSaga.ServiceDefaults.InternalServices.Middleware;
using PizzaSaga.Shared.ErrorHandling;
using PizzaSaga.Shared.Infrastructure.Persistence;
using Serilog;
using Stock.Infrastructure.DependencyInjection;
using Stock.Infrastructure.Persistence;

Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Подключаем автоматический OpenTelemetry, логирование и метрики Aspire
    builder.AddServiceDefaults();
    
    builder.Services.AddGlobalProblemDetailsExceptionHandling();

    // Стандартные сервисы

    // Infrastructure.
    // Регистрация DbContext. Название "StockDb" должно СТРОГО совпадать с именем ресурса в AppHost
    var dbConnectionString = builder.Configuration.GetDatabaseConnectionString("StockDb");
    var rabbitMqConnectionString = builder.Configuration.GetRabbitMqConnectionString();
    builder.Services.AddInfrastructure(dbConnectionString, rabbitMqConnectionString);


    var app = builder.Build();
    app.UseExceptionHandler();

    // Автоматические миграции и идемпотентный Seed данных. Вызов после app = builder.Build():
    await app.ApplyMigrationsAsync<StockDbContext>();

    // Пропагирует уже установленный CorrelationId: берёт из baggage или заголовка и добавляет в span-теги + логи.
    app.UseCorrelationId();

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
