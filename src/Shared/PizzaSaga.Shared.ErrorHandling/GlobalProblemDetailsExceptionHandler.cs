using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PizzaSaga.SharedKernel.Domain.Exceptions;

namespace PizzaSaga.Shared.ErrorHandling;

/// <summary>
/// Централизованно преобразует исключения приложения в HTTP ProblemDetails согласно RFC 9457.
/// </summary>
public sealed class GlobalProblemDetailsExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalProblemDetailsExceptionHandler> _logger;

    public GlobalProblemDetailsExceptionHandler(ILogger<GlobalProblemDetailsExceptionHandler> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var problemDetails = exception switch
        {
            BadHttpRequestException badRequestEx =>
                CreateBadRequestProblemDetails(httpContext, badRequestEx),

            ValidationException validationException =>
                CreateValidationProblemDetails(httpContext, validationException),

            DbUpdateConcurrencyException =>
                CreateConcurrencyProblemDetails(httpContext),

            DomainException domainException =>
                CreateDomainProblemDetails(httpContext, domainException),

            _ =>
                CreateInternalServerProblemDetails(httpContext)
        };

        LogException(
            httpContext,
            exception,
            problemDetails.Status!.Value);

        httpContext.Response.StatusCode = problemDetails.Status.Value;
        httpContext.Response.ContentType = "application/problem+json";

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }

    private static ProblemDetails CreateBadRequestProblemDetails(HttpContext httpContext, BadHttpRequestException exception)
    {
        var problemDetails = new ProblemDetails
        {
            Type = "urn:pizzasaga:error:request-validation",
            Title = "Violation of the request structure",
            Status = StatusCodes.Status400BadRequest,
            Instance = httpContext.Request.Path,
            Detail = exception.Message
        };

        AddTraceId(problemDetails);

        return problemDetails;
    }

    private static ProblemDetails CreateValidationProblemDetails(HttpContext httpContext, ValidationException exception)
    {
        var problemDetails = new ProblemDetails
        {
            Type = "urn:pizzasaga:error:request-validation",
            Title = "Violation of the request structure",
            Status = StatusCodes.Status400BadRequest,
            Instance = httpContext.Request.Path
        };

        var errors = exception.Errors
            .GroupBy(error => error.PropertyName)
            .ToDictionary(
                group => group.Key,
                group => group
                    .Select(error => error.ErrorMessage)
                    .Distinct()
                    .ToArray());

        problemDetails.Extensions["errors"] = errors;

        AddTraceId(problemDetails);

        return problemDetails;
    }

    private static ProblemDetails CreateDomainProblemDetails(HttpContext httpContext, DomainException exception)
    {
        var problemDetails = new ProblemDetails
        {
            Type = "urn:pizzasaga:error:business-rule",
            Title = "Violation of business rules",
            Status = StatusCodes.Status400BadRequest,
            Instance = httpContext.Request.Path,
            Detail = exception.Message
        };

        AddTraceId(problemDetails);

        return problemDetails;
    }

    private static ProblemDetails CreateConcurrencyProblemDetails(HttpContext httpContext)
    {
        var problemDetails = new ProblemDetails
        {
            Type = "urn:pizzasaga:error:concurrency",
            Title = "Parallel editing conflict",
            Status = StatusCodes.Status409Conflict,
            Instance = httpContext.Request.Path
        };

        AddTraceId(problemDetails);

        return problemDetails;
    }

    private static ProblemDetails CreateInternalServerProblemDetails(HttpContext httpContext)
    {
        var problemDetails = new ProblemDetails
        {
            Type = "urn:pizzasaga:error:internal-server-error",
            Title = "An error occurred while processing your request.",
            Status = StatusCodes.Status500InternalServerError,
            Instance = httpContext.Request.Path
        };

        AddTraceId(problemDetails);

        return problemDetails;
    }

    private void LogException(HttpContext httpContext, Exception exception, int statusCode)
    {
        switch (statusCode)
        {
            case StatusCodes.Status400BadRequest:
                _logger.LogInformation(exception, "Request failed with a client error. Path: {Path}", httpContext.Request.Path);
                break;

            case StatusCodes.Status409Conflict:
                _logger.LogWarning(exception, "Request failed due to a concurrency conflict. Path: {Path}", httpContext.Request.Path);
                break;

            default:
                _logger.LogError(exception, "Unhandled exception occurred while processing request. Path: {Path}", httpContext.Request.Path);
                break;
        }
    }

    private static void AddTraceId(ProblemDetails problemDetails)
    {
        var traceId = Activity.Current?.TraceId.ToString();

        if (!string.IsNullOrWhiteSpace(traceId))
        {
            problemDetails.Extensions["traceId"] = traceId;
        }
    }
}