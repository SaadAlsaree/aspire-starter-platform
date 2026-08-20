using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SharedKernel.Exceptions;
using SharedKernel.Results;

namespace SharedInfrastructure.Http;

internal sealed partial class GlobalExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (httpContext.Response.HasStarted)
        {
            return false;
        }

        if (exception is OperationCanceledException
            && httpContext.RequestAborted.IsCancellationRequested)
        {
            httpContext.Response.StatusCode = StatusCodes.Status499ClientClosedRequest;
            return true;
        }

        var problemDetails = Translate(exception, httpContext.Request.Path);
        httpContext.Response.StatusCode = problemDetails.Status!.Value;

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problemDetails,
            Exception = exception,
        });
    }

    private static HttpValidationProblemDetails ToValidationProblem(ValidationException exception) =>
        new HttpValidationProblemDetails(exception.Errors
            .GroupBy(failure => failure.PropertyName, StringComparer.Ordinal)
            .ToDictionary(
                group => group.Key,
                group => group.Select(failure => failure.ErrorMessage).ToArray(),
                StringComparer.Ordinal))
        {
            Status = StatusCodes.Status400BadRequest,
            Title = ErrorType.Validation.ToTitle(),
            Extensions = { ["code"] = "Validation.Failed" },
        };

    [LoggerMessage(
        EventId = 1000,
        Level = LogLevel.Information,
        Message = "Validation failed at {Path}")]
    private static partial void LogValidationFailed(ILogger logger, string path);

    [LoggerMessage(
        EventId = 1001,
        Level = LogLevel.Information,
        Message = "Domain failure at {Path}: {Code}")]
    private static partial void LogDomainFailure(ILogger logger, string path, string code);

    [LoggerMessage(
        EventId = 1002,
        Level = LogLevel.Warning,
        Message = "Persistence conflict at {Path}")]
    private static partial void LogPersistenceConflict(
        ILogger logger,
        Exception exception,
        string path);

    [LoggerMessage(
        EventId = 1003,
        Level = LogLevel.Error,
        Message = "Unhandled exception at {Path}")]
    private static partial void LogUnhandled(ILogger logger, Exception exception, string path);

    private ProblemDetails Translate(Exception exception, string path)
    {
        switch (exception)
        {
            case ValidationException validationException:
                LogValidationFailed(logger, path);
                return ToValidationProblem(validationException);

            case DomainException domainException:
                LogDomainFailure(logger, path, domainException.Error.Code);
                return domainException.Error.ToProblemDetails();

            case DbUpdateConcurrencyException:
                LogPersistenceConflict(logger, exception, path);
                return DomainError
                    .Conflict(
                        "Persistence.ConcurrencyConflict",
                        "The resource was modified by another request. Reload and retry.")
                    .ToProblemDetails();

            case DbUpdateException:
                LogPersistenceConflict(logger, exception, path);
                return DomainError
                    .Conflict(
                        "Persistence.Conflict",
                        "The operation conflicts with the current state of the resource.")
                    .ToProblemDetails();

            default:
                LogUnhandled(logger, exception, path);
                return DomainError
                    .Failure("Error.Unexpected", "An unexpected error occurred.")
                    .ToProblemDetails();
        }
    }
}
