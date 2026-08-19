using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace Microsoft.Extensions.Hosting;

public static class Extensions
{
    private const string ReadinessEndpointPath = "/health/ready";
    private const string LivenessEndpointPath = "/health/live";

    public static IHostApplicationBuilder AddServiceDefaults(
        this IHostApplicationBuilder builder)
    {
        builder.AddBasicServiceDefaults();
        builder.Services.AddServiceDiscovery();
        builder.Services.ConfigureHttpClientDefaults(httpClient =>
        {
            httpClient.AddStandardResilienceHandler();
            httpClient.AddServiceDiscovery();
        });

        return builder;
    }

    public static IHostApplicationBuilder AddBasicServiceDefaults(
        this IHostApplicationBuilder builder)
    {
        builder.AddDefaultHealthChecks();
        builder.ConfigureOpenTelemetry();

        return builder;
    }

    public static IHostApplicationBuilder ConfigureOpenTelemetry(
        this IHostApplicationBuilder builder)
    {
        builder.Logging.AddOpenTelemetry(logging =>
        {
            logging.IncludeFormattedMessage = true;
            logging.IncludeScopes = true;
        });

        builder.Services.AddOpenTelemetry()
            .WithMetrics(metrics =>
            {
                metrics.AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation();
            })
            .WithTracing(tracing =>
            {
                if (builder.Environment.IsDevelopment())
                {
                    tracing.SetSampler(new AlwaysOnSampler());
                }

                tracing.AddSource(builder.Environment.ApplicationName)
                    .AddAspNetCoreInstrumentation(options =>
                    {
                        options.Filter = context =>
                            !context.Request.Path.StartsWithSegments(
                                ReadinessEndpointPath)
                            && !context.Request.Path.StartsWithSegments(
                                LivenessEndpointPath);
                    })
                    .AddGrpcClientInstrumentation()
                    .AddHttpClientInstrumentation();
            });

        AddOpenTelemetryExporter(builder);

        return builder;
    }

    public static IHostApplicationBuilder AddDefaultHealthChecks(
        this IHostApplicationBuilder builder)
    {
        builder.Services.AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy(), ["live"]);

        return builder;
    }

    public static WebApplication MapDefaultEndpoints(this WebApplication app)
    {
        app.MapHealthChecks(ReadinessEndpointPath)
            .AllowAnonymous()
            .DisableRateLimiting();

        app.MapHealthChecks(
            LivenessEndpointPath,
            new HealthCheckOptions
            {
                Predicate = registration => registration.Tags.Contains("live"),
            })
            .AllowAnonymous()
            .DisableRateLimiting();

        return app;
    }

    private static void AddOpenTelemetryExporter(IHostApplicationBuilder builder)
    {
        if (!string.IsNullOrWhiteSpace(
            builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]))
        {
            builder.Services.AddOpenTelemetry().UseOtlpExporter();
        }
    }
}
