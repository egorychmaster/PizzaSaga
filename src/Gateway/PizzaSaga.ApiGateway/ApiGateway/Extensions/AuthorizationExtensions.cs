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
    /// Реализует простую, но надёжную логику: сначала проверяем путь, потом авторизуем. 
    /// Это позволяет легко масштабировать список публичных путей.
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
            bool isPublic = IsPublicPath(requestPath, publicPaths);

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

    /// <summary>
    /// Совпадает ли путь запроса с одним из публичных путей.
    ///
    /// Обычный путь работает как префикс по сегментам — как PathString.StartsWithSegments.
    /// Путь с параметром вида "v{version}" трактуется как шаблон в том же синтаксисе,
    /// что и маршруты YARP: литеральная часть сегмента обязательна, параметр заменяет
    /// его произвольную непустую часть. Поэтому "/api/v{version}/auth/login" покрывает
    /// и "/api/v1/auth/login", и будущий "/api/v2/auth/login" — добавление новой версии
    /// API не потребует менять список публичных путей Gateway.
    /// </summary>
    private static bool IsPublicPath(PathString requestPath, PathString[] publicPaths)
    {
        var requestSegments = SplitSegments(requestPath);

        foreach (var publicPath in publicPaths)
        {
            var patternSegments = SplitSegments(publicPath);

            // Публичный путь совпадает как префикс: "/swagger" открыт и для "/swagger/index.html".
            if (patternSegments.Length > requestSegments.Length)
                continue;

            var isMatch = true;

            for (var segmentIndex = 0; segmentIndex < patternSegments.Length; segmentIndex++)
            {
                if (!SegmentsMatch(patternSegments[segmentIndex], requestSegments[segmentIndex]))
                {
                    isMatch = false;
                    break;
                }
            }

            if (isMatch)
                return true;
        }

        return false;
    }

    /// <summary>
    /// Сопоставляет один сегмент пути с сегментом шаблона.
    /// Форма с параметром внутри сегмента ("v{version}") соответствует синтаксису
    /// маршрутов YARP: литеральная часть обязательна, параметр заменяет произвольную
    /// непустую часть сегмента. Сегмент без параметров сравнивается буквально.
    /// Такая форма нужна потому, что список версий API не должен становиться частью
    /// конфигурации авторизации Gateway.
    /// </summary>
    private static bool SegmentsMatch(string patternSegment, string requestSegment)
    {
        var parameterStart = patternSegment.IndexOf('{');

        if (parameterStart < 0)
            return string.Equals(patternSegment, requestSegment, StringComparison.OrdinalIgnoreCase);

        var parameterEnd = patternSegment.IndexOf('}', parameterStart);

        if (parameterEnd < 0)
            throw new InvalidOperationException($"Unclosed route parameter placeholder in public path segment '{patternSegment}'.");

        var prefix = patternSegment[..parameterStart];
        var suffix = patternSegment[(parameterEnd + 1)..];

        // Параметру должна соответствовать хотя бы одна буква.
        if (requestSegment.Length < prefix.Length + suffix.Length + 1)
            return false;

        return requestSegment.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
            && requestSegment.EndsWith(suffix, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Разбивает путь на сегменты без пустых элементов.
    /// </summary>
    private static string[] SplitSegments(PathString path)
    {
        var value = path.Value;

        if (string.IsNullOrEmpty(value))
            return [];

        return value!.Split('/', StringSplitOptions.RemoveEmptyEntries);
    }
}
