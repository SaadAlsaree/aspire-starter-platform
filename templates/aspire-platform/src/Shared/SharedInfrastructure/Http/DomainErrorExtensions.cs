using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Results;

namespace SharedInfrastructure.Http;

public static class DomainErrorExtensions
{
    public static int ToStatusCode(this ErrorType type) => type switch
    {
        ErrorType.Validation => StatusCodes.Status400BadRequest,
        ErrorType.Problem => StatusCodes.Status400BadRequest,
        ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
        ErrorType.Forbidden => StatusCodes.Status403Forbidden,
        ErrorType.NotFound => StatusCodes.Status404NotFound,
        ErrorType.Conflict => StatusCodes.Status409Conflict,
        _ => StatusCodes.Status500InternalServerError,
    };

    public static string ToTitle(this ErrorType type) => type switch
    {
        ErrorType.Validation => "Validation failed",
        ErrorType.Problem => "Request could not be processed",
        ErrorType.Unauthorized => "Unauthorized",
        ErrorType.Forbidden => "Forbidden",
        ErrorType.NotFound => "Resource not found",
        ErrorType.Conflict => "Conflict",
        _ => "An unexpected error occurred",
    };

    public static ProblemDetails ToProblemDetails(this DomainError error, int? statusCode = null)
    {
        ArgumentNullException.ThrowIfNull(error);

        return new ProblemDetails
        {
            Status = statusCode ?? error.Type.ToStatusCode(),
            Title = error.Type.ToTitle(),
            Detail = error.Description,
            Extensions =
            {
                ["code"] = error.Code,
                ["traceId"] = Activity.Current?.Id,
            },
        };
    }
}
