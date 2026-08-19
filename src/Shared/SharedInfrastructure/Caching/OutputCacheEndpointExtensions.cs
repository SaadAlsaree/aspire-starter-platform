using System.Security.Claims;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;
using SharedInfrastructure.Http;

namespace SharedInfrastructure.Caching;

public static class OutputCacheEndpointExtensions
{
    private const string AnonymousSubject = "anonymous";

    public static TBuilder CacheApiGet<TBuilder>(
        this TBuilder builder,
        string tag,
        int expirationSeconds = 60)
        where TBuilder : IEndpointConventionBuilder =>
        builder.CacheOutput(
            policy => policy
                .Expire(TimeSpan.FromSeconds(expirationSeconds))
                .Tag(tag)
                .With(context => HttpMethods.IsGet(context.HttpContext.Request.Method))
                .SetVaryByHeader(HeaderNames.Accept)
                .SetVaryByQuery(QueryKeys.Listing)
                .VaryByValue(context => new KeyValuePair<string, string>(
                    "sub",
                    context.User.FindFirstValue("sub") ?? AnonymousSubject)),
            excludeDefaultPolicy: true);
}
