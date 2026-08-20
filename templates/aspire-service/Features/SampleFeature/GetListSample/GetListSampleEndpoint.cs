using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using SharedApi.Policies;
using SharedInfrastructure.Collections;
using SharedInfrastructure.Http;
using Wolverine.Http;

namespace Simble.Features.SampleFeature.GetListSample;

public static class GetListSampleEndpoint
{
    [ApiVersion("1.0")]
    [Tags("Samples")]
    [EndpointSummary("List Samples")]
    [EndpointDescription("Get a paginated list of samples with filtering and sorting")]
    [WolverineGet("/samples")]
    [Cacheable(SampleCache.Tag)]
    [ProducesResponseType<PagedList<GetListSampleViewModel>>(StatusCodes.Status200OK)]
    public static async Task<IResult> ListAsync(
        [AsParameters] GetListSampleQuery query,
        GetListSampleHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.ExecuteAsync(query, cancellationToken);

        return result.ToHttpResult();
    }
}
