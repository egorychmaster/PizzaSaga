using Microsoft.AspNetCore.Http;

namespace PizzaSaga.Shared.ErrorHandling.Extensions;

public static class HttpResponseExtensions
{
    public static async Task WriteAsProblemDetailsAsync(
        this HttpResponse response,
        string type,
        string title,
        int status,
        string instance,
        Dictionary<string, object?>? extensions = null)
    {
        var problemDetails = new
        {
            Type = type,
            Title = title,
            Status = status,
            Instance = instance,
            Extensions = extensions ?? new Dictionary<string, object?>()
        };

        response.StatusCode = status;
        response.ContentType = "application/problem+json";

        // Используйте JsonSerializer для сериализации анонимного объекта
        await response.WriteAsJsonAsync(problemDetails);
    }

    public static async Task WriteAsProblemDetailsAsync(
        this HttpResponse response,
        string type,
        string title,
        int status,
        string instance,
        Exception exception)
    {
        var traceId = System.Diagnostics.Activity.Current?.TraceId.ToString()
                      ?? Guid.NewGuid().ToString();

        await response.WriteAsProblemDetailsAsync(
            type: type,
            title: title,
            status: status,
            instance: instance,
            extensions: new Dictionary<string, object?>
            {
                ["traceId"] = traceId
            });
    }
}