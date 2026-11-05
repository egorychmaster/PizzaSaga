using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace PizzaSaga.ServiceDefaults.Extensions;

public static class ProjectOpenApiExtensions
{
    /// <summary>
    /// Добавляет стандартную OpenAPI-документацию проекта и Swagger Bearer-аутентификацию.
    /// Генерирует OpenAPI-документ версии v1 по адресу /swagger/v1/swagger.json.
    /// Swagger UI может использовать Bearer JWT для выполнения защищённых API-запросов.
    /// Применять в сервисах, которые ещё НЕ перешли на API Versioning.
    /// </summary>
    public static WebApplicationBuilder AddProjectOpenApi(this WebApplicationBuilder builder)
    {
        builder.Services.AddEndpointsApiExplorer();

        builder.Services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = $"{builder.Environment.ApplicationName} API",
                Version = "v1"
            });

            AddBearerJwtSecurity(options);
        });

        return builder;
    }

    /// <summary>
    /// Добавляет OpenAPI-документацию вместе с API Versioning (URL Segment Versioning)
    /// и интеграцией API Explorer со Swashbuckle.
    /// Отличия от AddProjectOpenApi() ровно две:
    /// 1) endpoint'ы объявляют поддерживаемую версию явно через versioned route groups;
    /// 2) создаётся отдельный Swagger-документ для каждой РЕАЛЬНО реализованной версии API
    ///    (если реализована только v1 — публикуется только v1, пустой документ v2 не появляется).
    /// Публичный адрес документа не меняется: /swagger/{version}/swagger.json.
    /// </summary>
    public static WebApplicationBuilder AddProjectOpenApiWithVersioning(this WebApplicationBuilder builder)
    {
        builder.Services
            .AddApiVersioning(options =>
            {
                // Версия по умолчанию применяется только к endpoint'ам, которые НЕ объявляют
                // версию явно (то есть к инфраструктурным маршрутам без версии).
                options.DefaultApiVersion = new ApiVersion(1, 0);

                // Версия обязательна: запрос без сегмента версии (/api/orders) не должен
                // молча подставляться как v1.
                options.AssumeDefaultVersionWhenUnspecified = false;

                // Поддерживаемые версии отражаются в HTTP-заголовках ответов
                // (api-supported-versions / api-deprecated-versions).
                options.ReportApiVersions = true;

                // Версия определяется исключительно из URL-сегмента: /api/v1/...
                options.ApiVersionReader = new UrlSegmentApiVersionReader();
            })
            .AddApiExplorer(options =>
            {
                // Имя Swagger-документа формируется из версии: v1, v2 (минор в имя не входит).
                options.GroupNameFormat = "'v'VVV";

                // Обязательно: без этой настройки маршруты в swagger.json остаются литералом
                // "/api/v{version}/...", и Swagger UI не может выполнить запрос к endpoint'у.
                options.SubstituteApiVersionInUrl = true;
            });

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        // Конфигурация Swagger вынесена в IConfigureOptions, а не выполняется сразу в момент
        // регистрации служб: Minimal API endpoint'ы маппятся после builder.Build(), поэтому
        // список реализованных версий должен разрешаться из контейнера приложения уже после
        // формирования набора маршрутов. Разрешение провайдера на этапе конфигурации служб
        // дало бы пустой список версий (анализатор подсказывает об этом AV0027).
        builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>>(services =>
            new ProjectVersionedSwaggerGenOptions(
                services.GetRequiredService<IApiVersionDescriptionProvider>(),
                builder.Environment.ApplicationName));

        return builder;
    }

    /// <summary>
    /// Единое описание Bearer JWT для Swagger UI во всех сервисах.
    /// </summary>
    private static void AddBearerJwtSecurity(SwaggerGenOptions options)
    {
        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Введите JWT-токен в формате: Bearer {token}"
        });

        options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference("Bearer", document)] = []
        });
    }

    /// <summary>
    /// Swagger-конфигурация для API Versioning: по одному документу на каждую реализованную
    /// версию API плюс общая Bearer JWT-авторизация.
    /// </summary>
    private sealed class ProjectVersionedSwaggerGenOptions : IConfigureOptions<SwaggerGenOptions>
    {
        private readonly IApiVersionDescriptionProvider _apiVersionDescriptionProvider;
        private readonly string _applicationName;

        public ProjectVersionedSwaggerGenOptions(
            IApiVersionDescriptionProvider apiVersionDescriptionProvider,
            string applicationName)
        {
            _apiVersionDescriptionProvider = apiVersionDescriptionProvider;
            _applicationName = applicationName;
        }

        public void Configure(SwaggerGenOptions options)
        {
            foreach (var description in _apiVersionDescriptionProvider.ApiVersionDescriptions)
            {
                options.SwaggerDoc(description.GroupName, new OpenApiInfo
                {
                    Title = $"{_applicationName} API",
                    Version = description.GroupName
                });
            }

            AddBearerJwtSecurity(options);

            // В документ версии попадают только endpoint'ы, объявленные для этой версии.
            // Без этого predicate инфраструктурные маршруты без версии (health checks)
            // попадают в документ v1, что противоречит api.md.
            options.DocInclusionPredicate((documentName, apiDescription) =>
                apiDescription.GroupName == documentName);
        }
    }
}
