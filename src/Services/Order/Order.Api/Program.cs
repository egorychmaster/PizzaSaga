using Order.Api.Endpoints.Orders.CreateOrder;
using PizzaSaga.ServiceDefaults.Extensions;
using PizzaSaga.ServiceDefaults.InternalServices.Middleware;
using Serilog;

Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Подключаем автоматический OpenTelemetry, логирование и метрики Aspire
    builder.AddServiceDefaults();

    builder.AddJwtAuthentication();

    builder.Services.AddMediator();

    // ... твои стандартные сервисы ...

    var app = builder.Build();

    // Мидлварь аутентификации / авторизации
    app.UseAuthentication();
    app.UseAuthorization();

    // Пропагирует уже установленный CorrelationId: берёт из baggage или заголовка и добавляет в span-теги + логи.
    app.UseCorrelationId();


    // Настраиваем эндпоинты для проверки работоспособности (Health Checks)
    app.MapDefaultEndpoints();

    // Зарегистрировать endpoint
    app.MapCreateOrderEndpoint();

    app.MapGet("/api/v1/orders", () =>
    {
        //Console.WriteLine("=== ORDER ENDPOINT /api/orders ===");

        return new[]
        {
        new { Id = 1, CakeName = "Margarita", Status = "Pending" },
        new { Id = 2, CakeName = "Pepperoni", Status = "Baking" }
        };
    });


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