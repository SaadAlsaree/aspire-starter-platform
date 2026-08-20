using Microsoft.AspNetCore.Http;
using SharedKernel.Results;

namespace SharedInfrastructure.Http;

public static class ResultExtensions
{
    public static IResult ToHttpResult(this Result result)
    {
        ArgumentNullException.ThrowIfNull(result);

        return result.IsSuccess
            ? Results.NoContent()
            : Results.Problem(result.Error.ToProblemDetails());
    }

    public static IResult ToHttpResult<TValue>(this Result<TValue> result) =>
        result.ToHttpResult(Results.Ok);

    public static IResult ToHttpResult<TValue>(
        this Result<TValue> result,
        Func<TValue, IResult> onSuccess)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(onSuccess);

        return result.IsSuccess
            ? onSuccess(result.Value)
            : Results.Problem(result.Error.ToProblemDetails());
    }
}
