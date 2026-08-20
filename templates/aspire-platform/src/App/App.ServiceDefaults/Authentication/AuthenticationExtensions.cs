using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace App.ServiceDefaults.Authentication;

public static class AuthenticationExtensions
{
    /// <summary>
    /// Registers the bearer authentication selected by <c>Authentication:Provider</c> and returns
    /// the resolved provider so callers can decide whether to demand an authenticated user.
    /// </summary>
    public static AuthenticationProvider AddPlatformAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        var section = configuration.GetSection(PlatformAuthenticationOptions.SectionName);
        var settings = section.Get<PlatformAuthenticationOptions>()
            ?? new PlatformAuthenticationOptions();

        services.AddOptions<PlatformAuthenticationOptions>().Bind(section).ValidateOnStart();

        switch (settings.Provider)
        {
            case AuthenticationProvider.Keycloak:
                AddKeycloak(services, settings.Keycloak);
                break;

            case AuthenticationProvider.EntraId:
                AddEntraId(services, settings.EntraId);
                break;

            default:
                break;
        }

        return settings.Provider;
    }

    public static AuthenticationProvider GetAuthenticationProvider(this IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        return configuration
            .GetSection(PlatformAuthenticationOptions.SectionName)
            .GetValue(nameof(PlatformAuthenticationOptions.Provider), AuthenticationProvider.None);
    }

    private static void AddKeycloak(
        IServiceCollection services,
        KeycloakAuthenticationOptions settings)
    {
        Require(settings.Realm, "Keycloak:Realm");
        Require(settings.Audience, "Keycloak:Audience");

        var authentication = services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme);

        if (string.IsNullOrWhiteSpace(settings.Authority))
        {
            Require(settings.ServiceName, "Keycloak:ServiceName");

            authentication.AddKeycloakJwtBearer(
                serviceName: settings.ServiceName,
                realm: settings.Realm,
                options => ConfigureJwtBearer(
                    options,
                    settings.Audience,
                    settings.RequireHttpsMetadata,
                    settings.ClockSkew,
                    settings.ValidIssuers));
        }
        else
        {
            authentication.AddJwtBearer(options =>
            {
                options.Authority = settings.Authority.TrimEnd('/');
                ConfigureJwtBearer(
                    options,
                    settings.Audience,
                    settings.RequireHttpsMetadata,
                    settings.ClockSkew,
                    settings.ValidIssuers);
            });
        }
    }

    private static void AddEntraId(
        IServiceCollection services,
        EntraIdAuthenticationOptions settings)
    {
        Require(settings.TenantId, "EntraId:TenantId");
        Require(settings.Audience, "EntraId:Audience");

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = settings.Authority;
                ConfigureJwtBearer(
                    options,
                    settings.Audience,
                    settings.RequireHttpsMetadata,
                    settings.ClockSkew,
                    settings.ValidIssuers);
            });
    }

    private static void ConfigureJwtBearer(
        JwtBearerOptions options,
        string audience,
        bool requireHttpsMetadata,
        TimeSpan clockSkew,
        List<string> validIssuers)
    {
        options.Audience = audience;
        options.RequireHttpsMetadata = requireHttpsMetadata;
        options.MapInboundClaims = false;
        options.TokenValidationParameters.ClockSkew = clockSkew;

        if (validIssuers.Count > 0)
        {
            options.TokenValidationParameters.ValidIssuers = validIssuers;
        }
    }

    private static void Require(string value, string path)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException(
                $"{PlatformAuthenticationOptions.SectionName}:{path} is required for the selected authentication provider.");
        }
    }
}
