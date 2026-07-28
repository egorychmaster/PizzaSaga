using Order.Api.Endpoints.Orders.CreateOrder;
using Order.Api.Endpoints.Orders.GetOrders;
using Order.Infrastructure.Persistence;
using Order.Infrastructure.Persistence.Seeding;
using PizzaSaga.BuildingBlocks.Infrastructure.Persistence;
using PizzaSaga.ServiceDefaults.Extensions;
using PizzaSaga.ServiceDefaults.InternalServices.Middleware;
using Serilog;

Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Подключаем автоматический OpenTelemetry, логирование и метрики Aspire
    builder.AddServiceDefaults();
    builder.AddProjectOpenApi();

    builder.AddJwtAuthentication();

    builder.Services.AddMediator();

    // Стандартные сервисы 

    builder.Services.AddScoped<IDatabaseSeeder<OrderDbContext>, OrderDatabaseSeeder>();

    // Регистрация DbContext. Название "OrderDb" должно СТРОГО совпадать с именем ресурса в AppHost
    builder.AddNpgsqlDbContext<OrderDbContext>("OrderDb");


    var app = builder.Build();

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