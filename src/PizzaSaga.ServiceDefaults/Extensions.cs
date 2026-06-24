using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ServiceDiscovery;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace Microsoft.Extensions.Hosting
{
    // Adds common Aspire services: service discovery, resilience, health checks, and OpenTelemetry.
    // This project should be referenced by each service project in your solution.
    // To learn more about using this project, see https://aka.ms/dotnet/aspire/service-defaults
    public static class Extensions
    {
        private const string HealthEndpointPath = "/health";
        private const string AlivenessEndpointPath = "/alive";

        /// <summary>
        /// Подключает стандартные сервисы для всех компонентов .NET Aspire: 
        /// OpenTelemetry (трейсы, логи, метрики), health checks, service discovery и устойчивые HTTP-клиенты (Polly).
        /// Вызывается один раз в каждом сервисе через builder.AddServiceDefaults() в Program.cs.
        /// Является точкой входа к централизованной настройке поперечного функционала.
        /// </summary>
        public static TBuilder AddServiceDefaults<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
        {
            builder.ConfigureOpenTelemetry();

            builder.AddDefaultHealthChecks();

            builder.Services.AddServiceDiscovery();

            builder.Services.ConfigureHttpClientDefaults(http =>
            {
                // Turn on resilience by default
                http.AddStandardResilienceHandler();

                // Turn on service discovery by default
                http.AddServiceDiscovery();
            });

            // Uncomment the following to restrict the allowed schemes for service discovery.
            // builder.Services.Configure<ServiceDiscoveryOptions>(options =>
            // {
            //     options.AllowedSchemes = ["https"];
            // });

            return builder;
        }

        /// <summary>
        /// Настраивает OpenTelemetry: логирование с baggage, трейсы (ASP.NET Core, HttpClient), метрики (runtime, HTTP).
        /// Исключает запросы к /health и /alive из трейсинга — чтобы не засорять данные.
        /// Регистрирует OTLP-экспортер при наличии OTEL_EXPORTER_OTLP_ENDPOINT (используется Aspire Dashboard).
        /// Вызывается из AddServiceDefaults() → централизованная настройка observability.
        /// </summary>
        private static TBuilder ConfigureOpenTelemetry<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
        {
            builder.Logging.AddOpenTelemetry(logging =>
            {
                logging.IncludeFormattedMessage = true;
                logging.IncludeScopes = true;
            });

            builder.Services.AddOpenTelemetry()
                .WithMetrics(metrics =>
                {
                    metrics.AddAspNetCoreInstrumentation()
                        .AddHttpClientInstrumentation()
                        .AddRuntimeInstrumentation();
                })
                .WithTracing(tracing =>
                {
                    tracing.AddSource(builder.Environment.ApplicationName)
                        .AddAspNetCoreInstrumentation(tracing =>
                            // Exclude health check requests from tracing
                            tracing.Filter = context =>
                                !context.Request.Path.StartsWithSegments(HealthEndpointPath)
                                && !context.Request.Path.StartsWithSegments(AlivenessEndpointPath)
                        )
                        // Uncomment the following line to enable gRPC instrumentation (requires the OpenTelemetry.Instrumentation.GrpcNetClient package)
                        //.AddGrpcClientInstrumentation()
                        .AddHttpClientInstrumentation();
                });

            builder.AddOpenTelemetryExporters();

            return builder;
        }

        /// <summary>
        /// Добавляет OTLP-экспортер для отправки трейсов/метрик в центральный collector (по умолчанию — Aspire Dashboard на порту 4317).
        /// Активен только при заданной переменной OTEL_EXPORTER_OTLP_ENDPOINT.
        /// Инкапсулирует выбор экспортеров: можно легко добавить Azure Monitor и др. без дублирования кода в сервисах.
        /// </summary>
        private static TBuilder AddOpenTelemetryExporters<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
        {
            var useOtlpExporter = !string.IsNullOrWhiteSpace(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]);

            if (useOtlpExporter)
            {
                builder.Services.AddOpenTelemetry().UseOtlpExporter();
            }

            // Uncomment the following lines to enable the Azure Monitor exporter (requires the Azure.Monitor.OpenTelemetry.AspNetCore package)
            //if (!string.IsNullOrEmpty(builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"]))
            //{
            //    builder.Services.AddOpenTelemetry()
            //       .UseAzureMonitor();
            //}

            return builder;
        }

        /// <summary>
        /// Добавляет базовую liveness-проверку ("self"), помеченную тегом "live".
        /// Это минимальная гарантия, что процесс приложения работает — без проверки внешних зависимостей (БД, RabbitMQ и т.д.).
        /// Регистрируется в DI как IHealthChecksBuilder → используется в MapDefaultEndpoints() для привязки к эндпоинту /alive.
        /// В продакшене можно расширить добавлением проверок подключения к БД/RabbitMQ.
        /// </summary>
        private static TBuilder AddDefaultHealthChecks<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
        {
            builder.Services.AddHealthChecks()
                // Add a default liveness check to ensure app is responsive
                .AddCheck("self", () => HealthCheckResult.Healthy(), ["live"]);

            return builder;
        }

        /// <summary>
        /// Привязывает эндпоинты /health и /alive к приложению — только в Development (безопасность!).
        /// /health: все health checks, /alive: только тег "live" → используется оркестраторами (Docker/K8s) для liveness probe.
        /// Эндпоинты не трейсятся (исключаются через Filter в ConfigureOpenTelemetry) — избегаем шума в трейсах.
        /// Обязателен к вызову в Program.cs после построения WebApplication.
        /// </summary>
        public static WebApplication MapDefaultEndpoints(this WebApplication app)
        {
            // Adding health checks endpoints to applications in non-development environments has security implications.
            // See https://aka.ms/dotnet/aspire/healthchecks for details before enabling these endpoints in non-development environments.
            if (app.Environment.IsDevelopment())
            {
                // All health checks must pass for app to be considered ready to accept traffic after starting
                app.MapHealthChecks(HealthEndpointPath);

                // Only health checks tagged with the "live" tag must pass for app to be considered alive
                app.MapHealthChecks(AlivenessEndpointPath, new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("live")
                });
            }

            return app;
        }
    }
}
