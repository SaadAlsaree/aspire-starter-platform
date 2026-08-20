using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using SharedApi.Policies;
using SharedInfrastructure.Http;
using Wolverine.Http;

namespace Simble.Features.SampleFeature.GetByIdSample;

public static class GetByIdSampleEndpoint
{
    [ApiVersion("1.0")]
    [Tags("Samples")]
    [EndpointSummary("Get Sample")]
    [EndpointDescription("Get a single sample by its unique identifier")]
    [WolverineGet("/samples/{id}")]
    [Cacheable(SampleCache.Tag)]
    [ProducesResponseType<GetByIdSampleViewModel>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public static async Task<IResult> GetAsync(
        Guid id,
        GetByIdSampleHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.ExecuteAsync(new GetByIdSampleQuery(id), cancellationToken);

        return result.ToHttpResult();
    }
}
