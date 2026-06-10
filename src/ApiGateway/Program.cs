using System.Collections;

var builder = WebApplication.CreateBuilder(args);

foreach (DictionaryEntry env in Environment.GetEnvironmentVariables())
{
    Console.WriteLine($"{env.Key}={env.Value}");
}

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

// 2. Подключаем YARP-мидлварь для обработки входящих запросов
app.MapReverseProxy();


//app.MapGet("/test", async (IHttpClientFactory factory) =>
//{
//    var client = factory.CreateClient();

//    return await client.GetStringAsync(
//        "http://order-service/api/orders");
//});


app.Run();





//var builder = WebApplication.CreateBuilder(args);

//// Add services to the container.
//// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
//builder.Services.AddOpenApi();

//var app = builder.Build();

//// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.MapOpenApi();
//}

//app.UseHttpsRedirection();

//var summaries = new[]
//{
//    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
//};

//app.MapGet("/weatherforecast", () =>
//{
//    var forecast = Enumerable.Range(1, 5).Select(index =>
//        new WeatherForecast
//        (
//            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
//            Random.Shared.Next(-20, 55),
//            summaries[Random.Shared.Next(summaries.Length)]
//        ))
//        .ToArray();
//    return forecast;
//})
//.WithName("GetWeatherForecast");

//app.Run();

//internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
//{
//    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
//}
