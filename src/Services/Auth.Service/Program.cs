using Auth.Service.Extensions;
using System.Diagnostics;
using PizzaSaga.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

// Подключаем автоматический OpenTelemetry, логирование и метрики Aspire
builder.AddServiceDefaults();

// ... твои стандартные сервисы ...

var app = builder.Build();

// Настраиваем эндпоинты для проверки работоспособности (Health Checks)
app.MapDefaultEndpoints();


app.MapPost("/api/auth/login",
    async (LoginRequest req, IConfiguration config, ILogger<Program> logger) =>
{
    // MVP: жёстко заданный юзер для теста
    const string validEmail = "admin@test.com";
    const string validPassword = "password123";

    if (!string.Equals(req.Email, validEmail, StringComparison.Ordinal) ||
        !string.Equals(req.Password, validPassword, StringComparison.Ordinal))
    {
        logger.LogInformation("Login failed for user: {Email}", req.Email);
        return Results.Unauthorized(); // → 401
    }

    // Читается из конфига (appsettings.json или User Secrets)
    var secretKey = config["Jwt:SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");
    var token = AuthExtensions.GenerateJwtToken(req.Email, secretKey);

    logger.LogInformation("Login success for {Email}, correlation={Corr}",
        req.Email,
        GetCorrelationIdFromContext());

    return Results.Ok(new { Token = token });

    static string? GetCorrelationIdFromContext() =>
        Activity.Current?.Baggage.FirstOrDefault(x => x.Key == "correlation.id").Value;
})
.WithName("Login")
.Produces<Dictionary<string, object>>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status401Unauthorized);


app.Run();



public record LoginRequest(string Email, string Password);

