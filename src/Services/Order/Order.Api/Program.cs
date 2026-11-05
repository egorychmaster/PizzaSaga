using Order.Api.Endpoints.Orders.CreateOrder;
using Order.Api.Endpoints.Orders.GetOrderById;
using Order.Api.Endpoints.Orders.GetOrders;
using Order.Application.DependencyInjection;
using Order.Infrastructure.DependencyInjection;
using Order.Infrastructure.Persistence;
using PizzaSaga.ServiceDefaults.Extensions;
using PizzaSaga.ServiceDefaults.Extensions.Aspires;
using PizzaSaga.ServiceDefaults.InternalServices.Middleware;
using PizzaSaga.Shared.ErrorHandling;
using PizzaSaga.Shared.Infrastructure.Persistence;
using Serilog;

Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Подключаем автоматический OpenTelemetry, логирование и метрики Aspire
    builder.AddServiceDefaults();
    builder.AddProjectOpenApiWithVersioning();

    builder.AddJwtAuthentication();

    builder.Services.AddGlobalProblemDetailsExceptionHandling();

    // Стандартные сервисы 

    // Регистрирует зависимости слоя Application.
    builder.Services.AddApplication();

    // Order.Infrastructure.
    // Регистрация DbContext. Название "OrderDb" должно СТРОГО совпадать с именем ресурса в AppHost
    var connectionString = builder.Configuration.GetDatabaseConnectionString("OrderDb");
    var rabbitMqConnectionString = builder.Configuration.GetRabbitMqConnectionString();
    builder.Services.AddInfrastructure(connectionString, rabbitMqConnectionString);

    

    var app = builder.Build();
    app.UseExceptionHandler();

    // Только UseSwagger(), не UseSwaggerUI(), потому что service не обязан иметь собственный UI. Его задача — публиковать: /swagger/v1/swagger.json
    app.UseSwagger();

    // Автоматические миграции.
    await app.ApplyMigrationsAsync<OrderDbContext>();
    // Выполняем инициализацию данных
    await app.SeedDatabaseAsync<OrderDbContext>();

    // Мидлварь аутентификации / авторизации
    app.UseAuthentication();
    app.UseAuthorization();

    // Пропагирует уже установленный CorrelationId: берёт из baggage или заголовка и добавляет в span-теги + логи.
    app.UseCorrelationId();


    // Настраиваем эндпоинты для проверки работоспособности (Health Checks)
    app.MapDefaultEndpoints();

    // Зарегистрировать endpoint
    // API Versioning: URL Segment Versioning (/api/v{version}/...), версия обязательна.
    // Группа объявляет поддерживаемую версию 1.0 явно, поэтому все endpoint'ы заказов
    // попадают в Swagger-документ v1. Добавление v2 потребует отдельную группу
    // с HasApiVersion(2.0) и НЕ потребует менять маршруты v1.
    var v1Orders = app
        .NewVersionedApi()
        .MapGroup("/api/v{version:apiVersion}/orders")
        .HasApiVersion(1.0);

    v1Orders.MapCreateOrderEndpoint();
    v1Orders.MapGetOrdersEndpoint();
    v1Orders.MapGetOrderByIdEndpoint();


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