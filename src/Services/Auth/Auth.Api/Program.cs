using Auth.Api.Endpoints;
using Auth.Service.Extensions;
using PizzaSaga.ServiceDefaults.Extensions;
using PizzaSaga.ServiceDefaults.InternalServices.Middleware;
using Serilog;
using System.Diagnostics;

Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Подключаем автоматический OpenTelemetry, логирование и метрики Aspire
    builder.AddServiceDefaults();

    // ... твои стандартные сервисы ...

    var app = builder.Build();

    // Пропагирует уже установленный CorrelationId: берёт из baggage или заголовка и добавляет в span-теги + логи.
    app.UseCorrelationId();

    // Настраиваем эндпоинты для проверки работоспособности (Health Checks)
    app.MapDefaultEndpoints();


    app.MapLoginEndpoint();

    // Быстрый способ узнать точный хеш для любого пароля:
    string validHash = BCrypt.Net.BCrypt.HashPassword("password123", workFactor: 11);
    Console.WriteLine($"Generated Hash: {validHash}");


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


public record LoginRequest(string Email, string Password);

