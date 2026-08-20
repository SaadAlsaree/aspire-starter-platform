using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using SharedApi;
using Wolverine.Http;

namespace Simble.Features.SampleFeature.UpdateSample;

public static class UpdateSampleEndpoint
{
    [ApiVersion("1.0")]
    [Tags("Samples")]
    [EndpointSummary("Update Sample")]
    [EndpointDescription("Update an existing sample")]
    [WolverinePut("/samples/{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public static async Task<IResult> UpdateAsync(
        Guid id,
        UpdateSampleRequest request,
        [NotBody] UpdateSampleHandler handler,
        IOutputCacheStore cacheStore,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var command = new UpdateSampleCommand(id, request.Name, request.Description);
        var result = await handler.ExecuteAsync(command, cancellationToken);

        return await result.ToHttpResultAsync(cacheStore, SampleCache.Tag, cancellationToken);
    }
}
