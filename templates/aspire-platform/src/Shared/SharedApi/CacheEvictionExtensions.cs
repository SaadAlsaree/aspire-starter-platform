using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OutputCaching;
using SharedInfrastructure.Http;
using SharedKernel.Results;

namespace SharedApi;

/// <summary>
/// Translates a handler result into an HTTP response, evicting the feature's cached
/// reads whenever the write succeeded.
/// </summary>
public static class CacheEvictionExtensions
{
    public static async Task<IResult> ToHttpResultAsync(
        this Result result,
        IOutputCacheStore cacheStore,
        string cacheTag,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(cacheStore);

        if (result.IsSuccess)
        {
            await cacheStore.EvictByTagAsync(cacheTag, cancellationToken);
        }

        return result.ToHttpResult();
    }

    public static async Task<IResult> ToHttpResultAsync<TValue>(
        this Result<TValue> result,
        IOutputCacheStore cacheStore,
        string cacheTag,
        CancellationToken cancellationToken,
        Func<TValue, IResult>? onSuccess = null)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(cacheStore);

        if (result.IsSuccess)
        {
            await cacheStore.EvictByTagAsync(cacheTag, cancellationToken);
        }

        return onSuccess is null ? result.ToHttpResult() : result.ToHttpResult(onSuccess);
    }
}
