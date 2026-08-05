using Order.Api.Endpoints.Orders.CreateOrder;
using Order.Api.Endpoints.Orders.GetOrders;
using Order.Application.DependencyInjection;
using Order.Infrastructure.DependencyInjection;
using Order.Infrastructure.Persistence;
using Order.Infrastructure.Persistence.Seeding;
using PizzaSaga.ServiceDefaults.Extensions;
using PizzaSaga.ServiceDefaults.InternalServices.Middleware;
using PizzaSaga.Shared.Infrastructure.Persistence;
using Serilog;
using PizzaSaga.Shared.ErrorHandling;

Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Подключаем автоматический OpenTelemetry, логирование и метрики Aspire
    builder.AddServiceDefaults();
    builder.AddProjectOpenApi();

    builder.AddJwtAuthentication();

    builder.Services.AddGlobalProblemDetailsExceptionHandling();

    // Стандартные сервисы 

    builder.Services.AddOrderApplication();

    builder.Services.AddScoped<IDatabaseSeeder<OrderDbContext>, OrderDatabaseSeeder>();

    // Настройка Order.Infrastructure
    // Регистрация DbContext. Название "OrderDb" должно СТРОГО совпадать с именем ресурса в AppHost
    var connectionString = builder.Configuration.GetConnectionString("OrderDb");
    if (string.IsNullOrEmpty(connectionString))
        throw new InvalidOperationException("Connection string 'OrderDb' is not configured. Ensure WithReference(orderDb) is used in AppHost.");
    builder.Services.AddOrderInfrastructure(connectionString);


    var app = builder.Build();
    app.UseExceptionHandler();

    // Только UseSwagger(), не UseSwaggerUI(), потому что service не обязан иметь собственный UI. Его задача — публиковать: /swagger/v1/swagger.json
    app.UseSwagger();

    // Автоматические миграции и идемпотентный Seed данных. Вызов после app = builder.Build():
    await app.ApplyMigrationsAsync<OrderDbContext>();

    // Мидлварь аутентификации / авторизации
    app.UseAuthentication();
    app.UseAuthorization();

    // Пропагирует уже установленный CorrelationId: берёт из baggage или заголовка и добавляет в span-теги + логи.
    app.UseCorrelationId();


    // Настраиваем эндпоинты для проверки работоспособности (Health Checks)
    app.MapDefaultEndpoints();

    // Зарегистрировать endpoint
    app.MapCreateOrderEndpoint();
    app.MapGetOrdersEndpoint();


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