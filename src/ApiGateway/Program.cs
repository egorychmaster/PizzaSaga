using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using System.Diagnostics;
using System.Text;

var builder = WebApplication.CreateBuilder(args);


// Добавляем сервис-дефолты Aspire (OpenTelemetry, health checks, логирование и метрики и т.д.)
builder.AddServiceDefaults();


// 2. JWT: настройка валидации (пример для MVP — без реального Issuer/Audience)
var jwtSettings = builder.Configuration.GetSection("Jwt");
string? secretKey = jwtSettings["SecretKey"];
if (!string.IsNullOrEmpty(secretKey))
{
    var key = Encoding.UTF8.GetBytes(secretKey);

    builder.Services.AddAuthentication(options =>
    {
        // Говорим ASP.NET: «Использовать JWT-токен как стандартный механизм аутентификации».
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            // не issuer / audience — чтобы было проще запускать локально
            ValidateIssuer = false,         // Проверка, что токен выдан доверенным издателем (например, issuer: "https://auth.pizza-saga.com"). MVP: без проверки издателя
            ValidateAudience = false,       // Проверка, что токен предназначен именно для этого API (audience: "orders-api"). MVP: без проверки назначения

            ClockSkew = TimeSpan.Zero       // строгое сравнение времени
        };

        // 🔍 Обработка ошибок токена — в логах будет понятно, почему токен не прошёл (истёк? подделан? неверный ключ?).
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                {
                    Console.WriteLine("JWT token has expired.");
                }
                else
                {
                    Console.WriteLine("Authentication failed: " + context.Exception.Message);
                }
                return Task.CompletedTask;
            }
        };
    });

    // Глобальная политика по умолчанию (DefaultPolicy) - все endpoints  автоматически требуют авторизации, если не указано иное.
    builder.Services.AddAuthorization(options =>
    {
        options.DefaultPolicy = new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme)
            .RequireAuthenticatedUser()
            .Build();
    });
}
else
{
    // 💡 Для локального тестирования можно пропустить валидацию — но только в Development!
    if (builder.Environment.IsDevelopment())
    {
        Console.WriteLine("⚠️ JWT SecretKey not configured. JWT validation disabled for local dev.");
    }
}


// YARP + service discovery и связываем их с конфигурацией appsettings.json
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    // https://www.nuget.org/packages/Microsoft.Extensions.ServiceDiscovery.Yarp/
    // https://www.milanjovanovic.tech/blog/how-dotnet-aspire-simplifies-service-discovery
    // Configures a destination resolver that can use service discovery
    .AddServiceDiscoveryDestinationResolver();


var app = builder.Build();

// Корреляция: генерация + пропагация.
app.Use(async (ctx, next) =>
{
    if (!ctx.Request.Headers.TryGetValue("CorrelationId", out _))
    {
        ctx.Request.Headers["CorrelationId"] = Guid.CreateVersion7().ToString();
    }

    // Пропагируем в baggage для OpenTelemetry
    var cid = ctx.Request.Headers["CorrelationId"].ToString();
    Activity.Current?.AddBaggage("correlation.id", cid);

    // 🔍 Лог для отладки
    Console.WriteLine($"[Gateway] {ctx.Request.Method} {ctx.Request.Path}, Authorization: {ctx.Request.Headers.Authorization}");

    await next(ctx);
});

// Health checks — явно, с исключением из авторизации
app.MapHealthChecks("/health").AllowAnonymous();
app.MapHealthChecks("/alive", new HealthCheckOptions { Predicate = _ => true }).AllowAnonymous();

// Мидлварь аутентификации / авторизации
app.UseAuthentication();
app.UseAuthorization();

// Исключаем несколько публичных путей из проверки авторизации
var publicPaths = new[]
{
    new PathString("/api/auth/login"),   // Получить токен.

    new PathString("/health"),
    new PathString("/alive")
};
app.Use(async (ctx, next) =>
{
    var requestPath = ctx.Request.Path;

    // Проверяем: начинается ли путь с любого из публичных путей?
    bool isPublic = publicPaths.Any(publicPath =>
        requestPath.StartsWithSegments(publicPath, StringComparison.OrdinalIgnoreCase));
    if (isPublic)
    {
        await next(ctx); // без проверки!
        return;
    }

    // Для остальных — проверяем авторизацию через DefaultPolicy
    var policy = new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme)
        .RequireAuthenticatedUser()
        .Build();

    var authorizeResult = await ctx.RequestServices.GetRequiredService<IAuthorizationService>()
        .AuthorizeAsync(ctx.User, null, policy);

    if (!authorizeResult.Succeeded)
    {
        ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
        return;
    }

    await next(ctx);
});

// 4. YARP — последним
app.MapReverseProxy();


//app.MapGet("/test", async (IHttpClientFactory factory) =>
//{
//    var client = factory.CreateClient();
//    return await client.GetStringAsync("http://order-service/api/orders");
//});


app.Run();

