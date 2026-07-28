using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;

namespace PizzaSaga.ServiceDefaults.Extensions;

public static class ProjectOpenApiExtensions
{
    /// <summary>
    /// Добавляет стандартную OpenAPI-документацию проекта и Swagger Bearer-аутентификацию.
    /// Генерирует OpenAPI-документ версии v1 по адресу /swagger/v1/swagger.json.
    /// Swagger UI может использовать Bearer JWT для выполнения защищённых API-запросов.
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
        });

        return builder;
    }
}
