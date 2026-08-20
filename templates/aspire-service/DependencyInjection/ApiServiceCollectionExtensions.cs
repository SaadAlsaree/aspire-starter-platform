using System.Text.Json;
using System.Text.Json.Serialization;
using App.ServiceDefaults;
using App.ServiceDefaults.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Hosting;
using SharedApi;
using SharedInfrastructure.Http;
using SharedInfrastructure.RateLimiting;

namespace Simble.DependencyInjection;

public static class ApiServiceCollectionExtensions
{
    public const string DefaultCorsPolicy = "DefaultPolicy";
    public const string CacheConnectionName = "cache";

    public static IHostApplicationBuilder AddApiServices(
        this IHostApplicationBuilder builder,
        bool isCodeGeneration)
    {
        ArgumentNullException.ThrowIfNull(builder);

        var services = builder.Services;
        var configuration = builder.Configuration;

        services.AddDefaultOpenApi(configuration);
        services.AddApplicationForwardedHeaders(configuration);
        services.AddApplicationProblemDetails();
        services.AddApplicationRateLimiting(configuration);
        services.AddPlatformApi();
        AddCors(services, configuration, DefaultCorsPolicy);
        ConfigureJsonOptions(services);

        AddAuthorization(services, configuration, isCodeGeneration);
        AddOutputCache(builder);

        return builder;
    }

    private static void AddAuthorization(
        IServiceCollection services,
        IConfiguration configuration,
        bool isCodeGeneration)
    {
        var provider = isCodeGeneration
            ? AuthenticationProvider.None
            : services.AddPlatformAuthentication(configuration);

        var authorization = services.AddAuthorizationBuilder();

        if (provider != AuthenticationProvider.None)
        {
            authorization.SetFallbackPolicy(new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build());
        }
    }

    private static void AddOutputCache(IHostApplicationBuilder builder)
    {
        if (string.IsNullOrWhiteSpace(
            builder.Configuration.GetConnectionString(CacheConnectionName)))
        {
            builder.Services.AddOutputCache();
            return;
        }

        builder.AddRedisOutputCache(CacheConnectionName);
    }

    private static void AddCors(
        IServiceCollection services,
        IConfiguration configuration,
        string policyName)
    {
        var origins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
        if (origins.Length == 0)
        {
            throw new InvalidOperationException(
                "Cors:AllowedOrigins must contain at least one trusted origin.");
        }

        services.AddCors(options => options.AddPolicy(policyName, policy => policy
            .WithOrigins(origins)
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials()));
    }

    private static void ConfigureJsonOptions(IServiceCollection services) =>
        services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            options.SerializerOptions.DefaultIgnoreCondition =
                JsonIgnoreCondition.WhenWritingNull;
        });
}
