namespace SharedInfrastructure.Http;

public sealed class ForwardedHeadersSettings
{
    public const string SectionName = "ForwardedHeaders";

    public int ForwardLimit { get; set; } = 1;

    public IList<string> KnownProxies { get; set; } = [];

    public IList<string> KnownNetworks { get; set; } = [];
}
