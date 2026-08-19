using JasperFx;
using JasperFx.CodeGeneration;
using Microsoft.AspNetCore.Http;
using Wolverine.Http;

namespace SharedApi.Policies;

/// <summary>
/// Applied to every HTTP chain in every service so security metadata cannot be forgotten
/// on an individual endpoint.
/// </summary>
public sealed class SecurityHttpPolicy : IHttpPolicy
{
    public void Apply(
        IReadOnlyList<HttpChain> chains,
        GenerationRules rules,
        IServiceContainer container)
    {
        ArgumentNullException.ThrowIfNull(chains);

        foreach (var chain in chains)
        {
            chain.Metadata
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden)
                .ProducesProblem(StatusCodes.Status429TooManyRequests)
                .ProducesProblem(StatusCodes.Status500InternalServerError);
        }
    }
}
