using Payment.Infrastructure.DependencyInjection;
using Payment.Infrastructure.Persistence;
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
    //builder.AddProjectOpenApi(); // Swagger

    builder.Services.AddGlobalProblemDetailsExceptionHandling();

    // Infrastructure.
    // Регистрация зависимостей DbContext. Название "PaymentDb" должно СТРОГО совпадать с именем ресурса в AppHost
    var dbConnectionString = builder.Configuration.GetDatabaseConnectionString("PaymentDb");
    var rabbitMqConnectionString = builder.Configuration.GetRabbitMqConnectionString();
    builder.Services.AddInfrastructure(dbConnectionString, rabbitMqConnectionString);


    var app = builder.Build();
    app.UseExceptionHandler();

    // Пропагирует уже установленный CorrelationId: берёт из baggage или заголовка и добавляет в span-теги + логи.
    app.UseCorrelationId();

    // Настраиваем эндпоинты для проверки работоспособности (Health Checks)
    app.MapDefaultEndpoints();

    // Автоматические миграции.
    await app.ApplyMigrationsAsync<PaymentDbContext>();

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
