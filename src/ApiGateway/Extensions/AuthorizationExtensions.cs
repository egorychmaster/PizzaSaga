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
    /// <param name="app"></param>
    /// <param name="publicPaths">Cписок публичных маршрутов, не требующих JWT-токена</param>
    /// <returns></returns>
    public static IApplicationBuilder UsePublicPathAuthorization(this WebApplication app, PathString[] publicPaths)
    {
        app.Use(async (ctx, next) =>
        {
            // 1. Получаем текущий путь запроса
            var requestPath = ctx.Request.Path;

            // 2. Проверяем, входит ли путь в список "публичных"
            bool isPublic = publicPaths.Any(publicPath =>
                requestPath.StartsWithSegments(publicPath, StringComparison.OrdinalIgnoreCase));

            if (isPublic)
            {
                // Если да — просто пропускаем дальше (без проверки авторизации)
                await next(ctx);
                return;
            }

            // 3. Остальные пути — через DefaultPolicy
            var policy = new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme)
                .RequireAuthenticatedUser()
                .Build();

            var authorizeResult = await ctx.RequestServices.GetRequiredService<IAuthorizationService>()
                .AuthorizeAsync(ctx.User, null, policy);

            if (!authorizeResult.Succeeded)
            {
                // Если авторизация провалилась — возвращаем 401 явно
                ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return;
            }

            // 4. Если всё ок — передаём управление дальше по конвейеру
            await next(ctx);
        });

        return app;
    }
}