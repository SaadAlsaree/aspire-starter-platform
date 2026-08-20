using System.Globalization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SharedInfrastructure.Http;
using SharedKernel.Results;

namespace SharedInfrastructure.RateLimiting;

public static class RateLimitingServiceCollectionExtensions
{
    private const string UserPartitionPrefix = "user:";

    public static IServiceCollection AddApplicationRateLimiting(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        var section = configuration.GetSection(RateLimitingSettings.SectionName);
        var settings = section.Get<RateLimitingSettings>() ?? new RateLimitingSettings();

        Validate(settings);

        services.AddOptions<RateLimitingSettings>().Bind(section).ValidateOnStart();

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.OnRejected = (context, cancellationToken) =>
                WriteRejectionAsync(context, settings, cancellationToken);

            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(
                context => CreatePartition(context, settings));
        });

        return services;
    }

    private static RateLimitPartition<string> CreatePartition(
        HttpContext context,
        RateLimitingSettings settings)
    {
        var partitionKey = RateLimitPartitionKey.ForUserOrClient(context);

        return partitionKey.StartsWith(UserPartitionPrefix, StringComparison.Ordinal)
            ? RateLimitPartition.GetTokenBucketLimiter(
                partitionKey,
                _ => new TokenBucketRateLimiterOptions
                {
                    TokenLimit = settings.Authenticated.TokenLimit,
                    TokensPerPeriod = settings.Authenticated.TokensPerPeriod,
                    ReplenishmentPeriod = TimeSpan.FromSeconds(
                        settings.Authenticated.ReplenishmentPeriodSeconds),
                    AutoReplenishment = true,
                    QueueLimit = 0,
                })
            : RateLimitPartition.GetSlidingWindowLimiter(
                partitionKey,
                _ => new SlidingWindowRateLimiterOptions
                {
                    PermitLimit = settings.Anonymous.PermitLimit,
                    Window = TimeSpan.FromSeconds(settings.Anonymous.WindowSeconds),
                    SegmentsPerWindow = settings.Anonymous.SegmentsPerWindow,
                    AutoReplenishment = true,
                    QueueLimit = 0,
                });
    }

    private static async ValueTask WriteRejectionAsync(
        OnRejectedContext context,
        RateLimitingSettings settings,
        CancellationToken cancellationToken)
    {
        var retryAfterSeconds =
            context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter)
                ? Math.Max(1, (int)Math.Ceiling(retryAfter.TotalSeconds))
                : settings.Anonymous.WindowSeconds;

        var httpContext = context.HttpContext;
        httpContext.Response.Headers.RetryAfter =
            retryAfterSeconds.ToString(CultureInfo.InvariantCulture);

        await httpContext.Response.WriteAsJsonAsync(
            DomainError
                .Problem("RateLimit.Exceeded", "Too many requests. Please retry later.")
                .ToProblemDetails(StatusCodes.Status429TooManyRequests),
            cancellationToken);
    }

    private static void Validate(RateLimitingSettings settings)
    {
        ValidatePositive(
            $"{RateLimitingSettings.SectionName}:Authenticated:TokenLimit",
            settings.Authenticated.TokenLimit);
        ValidatePositive(
            $"{RateLimitingSettings.SectionName}:Authenticated:TokensPerPeriod",
            settings.Authenticated.TokensPerPeriod);
        ValidatePositive(
            $"{RateLimitingSettings.SectionName}:Authenticated:ReplenishmentPeriodSeconds",
            settings.Authenticated.ReplenishmentPeriodSeconds);

        if (settings.Authenticated.TokensPerPeriod > settings.Authenticated.TokenLimit)
        {
            throw new InvalidOperationException(
                $"{RateLimitingSettings.SectionName}:Authenticated:TokensPerPeriod cannot exceed TokenLimit.");
        }

        ValidatePositive(
            $"{RateLimitingSettings.SectionName}:Anonymous:PermitLimit",
            settings.Anonymous.PermitLimit);
        ValidatePositive(
            $"{RateLimitingSettings.SectionName}:Anonymous:WindowSeconds",
            settings.Anonymous.WindowSeconds);
        ValidatePositive(
            $"{RateLimitingSettings.SectionName}:Anonymous:SegmentsPerWindow",
            settings.Anonymous.SegmentsPerWindow);

        if (settings.Anonymous.SegmentsPerWindow > settings.Anonymous.WindowSeconds)
        {
            throw new InvalidOperationException(
                $"{RateLimitingSettings.SectionName}:Anonymous:SegmentsPerWindow cannot exceed WindowSeconds.");
        }
    }

    private static void ValidatePositive(string path, int value)
    {
        if (value <= 0)
        {
            throw new InvalidOperationException($"{path} must be greater than zero.");
        }
    }
}
