using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using SharedApi;
using Wolverine.Http;

namespace Simble.Features.SampleFeature.SoftDeleteSample;

public static class SoftDeleteSampleEndpoint
{
    [ApiVersion("1.0")]
    [Tags("Samples")]
    [EndpointSummary("Delete Sample")]
    [EndpointDescription("Soft delete a sample so it is hidden from reads but retained for audit")]
    [WolverineDelete("/samples/{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public static async Task<IResult> DeleteAsync(
        Guid id,
        [NotBody] SoftDeleteSampleHandler handler,
        IOutputCacheStore cacheStore,
        CancellationToken cancellationToken)
    {
        var result = await handler.ExecuteAsync(
            new SoftDeleteSampleCommand(id),
            cancellationToken);

        return await result.ToHttpResultAsync(cacheStore, SampleCache.Tag, cancellationToken);
    }
}
