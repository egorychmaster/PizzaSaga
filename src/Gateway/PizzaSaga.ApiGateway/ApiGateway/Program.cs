using ApiGateway.Extensions;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using PizzaSaga.ServiceDefaults.Extensions;
using Serilog;

Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Добавляем сервис-дефолты Aspire (OpenTelemetry, health checks, логирование и метрики и т.д.)
    builder.AddServiceDefaults();


    // JWT: настройка валидации
    builder.AddJwtAuthentication();


    // YARP + service discovery и связываем их с конфигурацией appsettings.json
    builder.Services.AddReverseProxy()
        .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
        // https://www.nuget.org/packages/Microsoft.Extensions.ServiceDiscovery.Yarp/
        // https://www.milanjovanovic.tech/blog/how-dotnet-aspire-simplifies-service-discovery
        // Configures a destination resolver that can use service discovery
        .AddServiceDiscoveryDestinationResolver();


    var app = builder.Build();

    app.UseGlobalExceptionHandling();

    // Корреляция: генерация + пропагация.
    app.UseCorrelationId();


    // Health checks — явно, с исключением из авторизации
    app.MapHealthChecks("/health").AllowAnonymous();
    app.MapHealthChecks("/alive", new HealthCheckOptions { Predicate = _ => true }).AllowAnonymous();

    // Мидлварь аутентификации / авторизации
    app.UseAuthentication();
    app.UseAuthorization();

    // Исключаем несколько публичных путей из проверки авторизации (нет JWT-токена)
    var publicPaths = new[]
    {
        new PathString("/api/v1/auth/login"),   // Получение токена
        new PathString("/test"),
        new PathString("/health"),
        new PathString("/alive")
    };
    app.UsePublicPathAuthorization(publicPaths);

    // 4. YARP — последним
    app.MapReverseProxy();


    app.MapGet("/test", async (IHttpClientFactory factory) =>
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