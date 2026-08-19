namespace SharedApi.Policies;

[AttributeUsage(AttributeTargets.Method)]
public sealed class CacheableAttribute(string tag, int expirationSeconds = 60) : Attribute
{
    public string Tag { get; } = tag;

    public int ExpirationSeconds { get; } = expirationSeconds;
}
