using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;


namespace PizzaSaga.ServiceDefaults.Extensions;

public static class JwtAuthenticationExtensions
{
    /// <summary>
    /// Единая точка настройки JWT Bearer аутентификации для всех сервисов системы (Zero Trust).
    /// Добавляет конфигурацию JWT Bearer-аутентификации.
    /// Использует конфигурационную секцию "Jwt" → SecretKey.
    /// </summary>
    /// <param name="builder">Билдер приложения.</param>
    public static WebApplicationBuilder AddJwtAuthentication(this WebApplicationBuilder builder)
    {
        var jwtSettings = builder.Configuration.GetSection("Jwt");
        var secretKey = jwtSettings["SecretKey"];

        if (string.IsNullOrEmpty(secretKey))
            throw new Exception("The secret key for generating the JWT token has not been set.");

        var key = Encoding.UTF8.GetBytes(secretKey);

        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme   = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            // Валидация ключа и подписи
            options.TokenValidationParameters = new TokenValidationParameters
            {
                // Проверять подпись
                ValidateIssuerSigningKey = true,
                // Ключ для проверки подписи
                IssuerSigningKey         = new SymmetricSecurityKey(key),
                // Для локального MVP: отключены проверки issuer/audience
                ValidateIssuer           = false,
                // Отключаем обязательную проверку срока годности токена
                ValidateLifetime         = false,
                ValidateAudience         = false,
                ClockSkew                = TimeSpan.Zero
            };

            // Детализированная обработка ошибок токена (для отладки и мониторинга)
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
                        Console.WriteLine($"Authentication failed: {context.Exception.Message}");
                    }

                    return Task.CompletedTask;
                }
            };
        });

        // Устанавливаем глобальную политику по умолчанию — все endpoints требуют аутентификации,
        // если не указано иное (через [Authorize] или публичные маршруты).
        builder.Services.AddAuthorization(options =>
        {
            options.DefaultPolicy = new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme)
                .RequireAuthenticatedUser()
                .Build();
        });

        return builder;
    }
}
