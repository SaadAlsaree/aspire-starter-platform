using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using SharedApi;
using Wolverine.Http;

namespace Simble.Features.SampleFeature.CreateSample;

public static class CreateSampleEndpoint
{
    [ApiVersion("1.0")]
    [Tags("Samples")]
    [EndpointSummary("Create Sample")]
    [EndpointDescription("Create a new sample")]
    [WolverinePost("/samples")]
    [ProducesResponseType<Guid>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public static async Task<IResult> CreateAsync(
        CreateSampleCommand command,
        [NotBody] CreateSampleHandler handler,
        IOutputCacheStore cacheStore,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var result = await handler.ExecuteAsync(command, cancellationToken);

        return await result.ToHttpResultAsync(
            cacheStore,
            SampleCache.Tag,
            cancellationToken,
            id => Results.Created($"{httpContext.Request.Path}/{id}", id));
    }
}
