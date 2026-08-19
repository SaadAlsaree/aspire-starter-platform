using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using Scalar.AspNetCore;

namespace App.ServiceDefaults;

public static class OpenApiExtensions
{
    public static IServiceCollection AddDefaultOpenApi(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var section = configuration.GetSection(OpenApiSettings.SectionName);
        var settings = section.Get<OpenApiSettings>() ?? new OpenApiSettings();

        services.AddOptions<OpenApiSettings>()
            .Bind(section)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOpenApi(settings.DocumentName, options =>
        {
            options.AddDocumentTransformer((document, _, _) =>
            {
                document.Info.Title = settings.Title;
                document.Info.Description = settings.Description;
                document.Info.Version = settings.DocumentName;

                if (string.IsNullOrWhiteSpace(settings.OAuthAuthority))
                {
                    return Task.CompletedTask;
                }

                var authority = settings.OAuthAuthority.TrimEnd('/');
                var securityScheme = new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.OAuth2,
                    Flows = new OpenApiOAuthFlows
                    {
                        AuthorizationCode = new OpenApiOAuthFlow
                        {
                            AuthorizationUrl = new Uri(
                                $"{authority}/protocol/openid-connect/auth"),
                            TokenUrl = new Uri(
                                $"{authority}/protocol/openid-connect/token"),
                            Scopes = settings.Scopes,
                        },
                    },
                };

                document.Components ??= new OpenApiComponents();
                document.Components.SecuritySchemes ??=
                    new Dictionary<string, IOpenApiSecurityScheme>();
                document.Components.SecuritySchemes["oauth2"] = securityScheme;

                return Task.CompletedTask;
            });

            options.AddOperationTransformer((operation, context, _) =>
            {
                var metadata = context.Description.ActionDescriptor.EndpointMetadata;
                var requiresAuthorization = metadata.OfType<IAuthorizeData>().Any()
                    && !metadata.OfType<IAllowAnonymous>().Any();

                if (!requiresAuthorization
                    || string.IsNullOrWhiteSpace(settings.OAuthAuthority))
                {
                    return Task.CompletedTask;
                }

                operation.Responses ??= new OpenApiResponses();
                operation.Responses.TryAdd(
                    "401",
                    new OpenApiResponse { Description = "Unauthorized" });
                operation.Responses.TryAdd(
                    "403",
                    new OpenApiResponse { Description = "Forbidden" });
                operation.Security =
                [
                    new OpenApiSecurityRequirement
                    {
                        [new OpenApiSecuritySchemeReference("oauth2", null)] =
                            settings.Scopes.Keys.ToList(),
                    },
                ];

                return Task.CompletedTask;
            });
        });

        return services;
    }

    public static WebApplication MapDefaultOpenApi(this WebApplication app)
    {
        var settings = app.Services.GetRequiredService<IOptions<OpenApiSettings>>().Value;
        if (!app.Environment.IsDevelopment() && !settings.ExposeInProduction)
        {
            return app;
        }

        app.MapOpenApi().AllowAnonymous();
        app.MapScalarApiReference(options =>
        {
            options.WithTitle(settings.Title);

            if (!string.IsNullOrWhiteSpace(settings.OAuthClientId))
            {
                options.AddAuthorizationCodeFlow("oauth2", oauth =>
                {
                    oauth.ClientId = settings.OAuthClientId;
                    oauth.Pkce = Pkce.Sha256;
                    oauth.SelectedScopes = settings.Scopes.Keys.ToArray();
                });
            }
        }).AllowAnonymous();

        return app;
    }
}
