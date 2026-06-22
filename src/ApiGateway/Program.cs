using System.Collections;
using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

// Подключаем автоматический OpenTelemetry, логирование и метрики Aspire
builder.AddServiceDefaults();

// ... твои стандартные сервисы ...
// 1. Добавляем сервисы YARP и связываем их с конфигурацией appsettings.json
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    // https://www.nuget.org/packages/Microsoft.Extensions.ServiceDiscovery.Yarp/
    // https://www.milanjovanovic.tech/blog/how-dotnet-aspire-simplifies-service-discovery
    // Configures a destination resolver that can use service discovery
    .AddServiceDiscoveryDestinationResolver();

var app = builder.Build();

// Настраиваем эндпоинты для проверки работоспособности (Health Checks)
app.MapDefaultEndpoints();

// Корреляция: генерация + пропагация
app.Use(async (ctx, next) =>
{
    if (!ctx.Request.Headers.TryGetValue("CorrelationId", out _))
    {
        ctx.Request.Headers["CorrelationId"] = Guid.CreateVersion7().ToString();
    }

    // Пропагируем в baggage для OpenTelemetry
    var cid = ctx.Request.Headers["CorrelationId"].ToString();
    Activity.Current?.AddBaggage("correlation.id", cid);

    await next(ctx);
});

// 2. Подключаем YARP-мидлварь для обработки входящих запросов
app.MapReverseProxy();


//app.MapGet("/test", async (IHttpClientFactory factory) =>
//{
//    var client = factory.CreateClient();
//    return await client.GetStringAsync("http://order-service/api/orders");
//});


app.Run();

