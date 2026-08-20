namespace SharedInfrastructure.RateLimiting;

public sealed class RateLimitingSettings
{
    public const string SectionName = "RateLimiting";

    public TokenBucketSettings Authenticated { get; set; } = new();

    public SlidingWindowSettings Anonymous { get; set; } = new();

    public sealed class TokenBucketSettings
    {
        public int TokenLimit { get; set; } = 60;

        public int TokensPerPeriod { get; set; } = 10;

        public int ReplenishmentPeriodSeconds { get; set; } = 10;
    }

    public sealed class SlidingWindowSettings
    {
        public int PermitLimit { get; set; } = 120;

        public int WindowSeconds { get; set; } = 60;

        public int SegmentsPerWindow { get; set; } = 6;
    }
}
