using Catalog.Infrastructure.Persistence;
using Catalog.Infrastructure.DependencyInjection;
using PizzaSaga.ServiceDefaults.Extensions;
using PizzaSaga.Shared.ErrorHandling;
using PizzaSaga.Shared.Infrastructure.Persistence;
using Serilog;

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

    // Catalog.Infrastructure.
    // Регистрация DbContext. Название "CatalogDb" должно СТРОГО совпадать с именем ресурса в AppHost
    var connectionString = builder.Configuration.GetConnectionString("CatalogDb");
    if (string.IsNullOrEmpty(connectionString))
        throw new InvalidOperationException("Connection string 'CatalogDb' is not configured. Ensure WithReference(orderDb) is used in AppHost.");
    builder.Services.AddOrderInfrastructure(connectionString);


    var app = builder.Build();
    app.UseExceptionHandler();

    // Только UseSwagger(), не UseSwaggerUI(), потому что service не обязан иметь собственный UI. Его задача — публиковать: /swagger/v1/swagger.json
    app.UseSwagger();

    // Автоматические миграции и идемпотентный Seed данных. Вызов после app = builder.Build():
    await app.ApplyMigrationsAsync<CatalogDbContext>();

    // Настраиваем эндпоинты для проверки работоспособности (Health Checks)
    app.MapDefaultEndpoints();


    app.MapGet("/api/v1/catalogs/test2", async (IHttpClientFactory factory) =>
    {
        throw new Exception("My test mistake.");

        var client = factory.CreateClient();
        return await client.GetStringAsync("http://order-service/api/orders");
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

