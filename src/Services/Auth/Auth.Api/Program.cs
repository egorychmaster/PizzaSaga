using Auth.Api.Endpoints;
using PizzaSaga.ServiceDefaults.Extensions;
using PizzaSaga.ServiceDefaults.InternalServices.Middleware;
using PizzaSaga.Shared.ErrorHandling;
using Serilog;

Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Подключаем автоматический OpenTelemetry, логирование и метрики Aspire
    builder.AddServiceDefaults();
    // OpenAPI + API Versioning: отдельный Swagger-документ на каждую реализованную версию (v1).
    builder.AddProjectOpenApiWithVersioning();

    builder.Services.AddGlobalProblemDetailsExceptionHandling();


    // ... Стандартные сервисы ...

    var app = builder.Build();

    app.UseExceptionHandler();

    // Только UseSwagger(), не UseSwaggerUI(), потому что service не обязан иметь собственный UI. Его задача — публиковать: /swagger/v1/swagger.json
    app.UseSwagger();

    // Пропагирует уже установленный CorrelationId: берёт из baggage или заголовка и добавляет в span-теги + логи.
    app.UseCorrelationId();

    // Настраиваем эндпоинты для проверки работоспособности (Health Checks)
    app.MapDefaultEndpoints();


    // API Versioning: URL Segment Versioning (/api/v{version}/...), версия обязательна.
    // Группа объявляет поддерживаемую версию 1.0 явно, поэтому endpoint аутентификации
    // попадает в Swagger-документ v1. Добавление v2 потребует отдельную группу
    // с HasApiVersion(2.0) и НЕ потребует менять маршруты v1.
    var v1Auth = app
        .NewVersionedApi()
        .MapGroup("/api/v{version:apiVersion}/auth")
        .HasApiVersion(1.0);

    v1Auth.MapLoginEndpoint();

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


