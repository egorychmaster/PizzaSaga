using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;

namespace ApiGateway.Extensions;

/// <summary>
/// Расширения для настройки публичных (открытых) путей и централизованной авторизации.
/// Позволяет легко добавлять новые "белые" пути без дублирования логики.
///
/// Рекомендуется использовать вместо hard-coded массива `publicPaths` в Program.cs.
/// </summary>
public static class AuthorizationExtensions
{
    /// <summary>
    /// Подключает middleware, проверяющий:
    /// - является ли текущий путь "публичным" (не требует аутентификации),
    /// - если нет — применяет DefaultPolicy (требует аутентифицированного пользователя).
    ///
    /// Реализует простую, но надёжную логику: сначала проверяем путь,
    /// потом авторизуем. Это позволяет легко масштабировать список публичных путей.
    /// </summary>
    public static IApplicationBuilder UsePublicPathAuthorization(this WebApplication app)
    {
        // ПУБЛИЧНЫЕ маршруты — не требуют JWT-токена
        var publicPaths = new[]
        {
            new PathString("/api/auth/login"),   // Получение токена
            new PathString("/health"),
            new PathString("/alive")
        };

        app.Use(async (ctx, next) =>
        {
            var requestPath = ctx.Request.Path;

            bool isPublic = publicPaths.Any(publicPath =>
                requestPath.StartsWithSegments(publicPath, StringComparison.OrdinalIgnoreCase));

            if (isPublic)
            {
                await next(ctx); // Пропускаем авторизацию
                return;
            }

            // Остальные пути — через DefaultPolicy
            var policy = new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme)
                .RequireAuthenticatedUser()
                .Build();

            var authorizeResult = await ctx.RequestServices.GetRequiredService<IAuthorizationService>()
                .AuthorizeAsync(ctx.User, null, policy);

            if (!authorizeResult.Succeeded)
            {
                // Возвращаем 401 в явном виде (стандартный ASP.NET вернёт 401 при вызове challenge,
                // но здесь мы хотим контролировать поведение — например, для YARP-прокси это важно).
                ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return;
            }

            await next(ctx);
        });

        return app;
    }
}