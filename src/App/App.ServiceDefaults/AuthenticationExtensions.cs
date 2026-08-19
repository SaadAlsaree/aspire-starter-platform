using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace App.ServiceDefaults;

public static class AuthenticationExtensions
{
    public static IServiceCollection AddKeycloakAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var section = configuration.GetRequiredSection(KeycloakAuthenticationOptions.SectionName);
        var settings = section.Get<KeycloakAuthenticationOptions>()
            ?? throw new InvalidOperationException(
                $"Configuration section '{KeycloakAuthenticationOptions.SectionName}' is invalid.");

        Validate(settings);

        services.AddOptions<KeycloakAuthenticationOptions>()
            .Bind(section)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        var authentication = services.AddAuthentication(
            JwtBearerDefaults.AuthenticationScheme);

        if (string.IsNullOrWhiteSpace(settings.Authority))
        {
            authentication.AddKeycloakJwtBearer(
                serviceName: settings.ServiceName,
                realm: settings.Realm,
                options => ConfigureJwtBearer(options, settings));
        }
        else
        {
            authentication.AddJwtBearer(options =>
            {
                options.Authority = settings.Authority.TrimEnd('/');
                ConfigureJwtBearer(options, settings);
            });
        }

        services.AddAuthorizationBuilder();

        return services;
    }

    private static void ConfigureJwtBearer(
        JwtBearerOptions options,
        KeycloakAuthenticationOptions settings)
    {
        options.Audience = settings.Audience;
        options.RequireHttpsMetadata = settings.RequireHttpsMetadata;
        options.MapInboundClaims = false;

        if (settings.ValidIssuers.Count > 0)
        {
            options.TokenValidationParameters.ValidIssuers = settings.ValidIssuers;
        }

        options.TokenValidationParameters.ClockSkew = settings.ClockSkew;
    }

    private static void Validate(KeycloakAuthenticationOptions settings)
    {
        if (string.IsNullOrWhiteSpace(settings.ServiceName))
        {
            throw new InvalidOperationException(
                $"{KeycloakAuthenticationOptions.SectionName}:ServiceName is required.");
        }

        if (string.IsNullOrWhiteSpace(settings.Realm))
        {
            throw new InvalidOperationException(
                $"{KeycloakAuthenticationOptions.SectionName}:Realm is required.");
        }

        if (string.IsNullOrWhiteSpace(settings.Audience))
        {
            throw new InvalidOperationException(
                $"{KeycloakAuthenticationOptions.SectionName}:Audience is required.");
        }
    }
}
