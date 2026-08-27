using PizzaSaga.ServiceDefaults.Extensions;
using PizzaSaga.Shared.ErrorHandling;
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

    var app = builder.Build();
    app.UseExceptionHandler();

    // Только UseSwagger(), не UseSwaggerUI(), потому что service не обязан иметь собственный UI. Его задача — публиковать: /swagger/v1/swagger.json
    app.UseSwagger();

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

