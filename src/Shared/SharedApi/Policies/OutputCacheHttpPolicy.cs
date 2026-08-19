using JasperFx;
using JasperFx.CodeGeneration;
using SharedInfrastructure.Caching;
using Wolverine.Http;

namespace SharedApi.Policies;

/// <summary>
/// Applies the platform output cache policy to every chain marked with <see cref="CacheableAttribute"/>.
/// </summary>
public sealed class OutputCacheHttpPolicy : IHttpPolicy
{
    public void Apply(
        IReadOnlyList<HttpChain> chains,
        GenerationRules rules,
        IServiceContainer container)
    {
        ArgumentNullException.ThrowIfNull(chains);

        foreach (var chain in chains)
        {
            var cacheable = chain.Method.Method
                .GetCustomAttributes(typeof(CacheableAttribute), inherit: true)
                .OfType<CacheableAttribute>()
                .FirstOrDefault();

            if (cacheable is not null)
            {
                chain.Metadata.CacheApiGet(cacheable.Tag, cacheable.ExpirationSeconds);
            }
        }
    }
}
