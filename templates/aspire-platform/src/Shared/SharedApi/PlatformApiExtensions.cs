using Asp.Versioning;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using SharedApi.Policies;
using Wolverine.Http;
using Wolverine.Http.ApiVersioning;
using Wolverine.Http.FluentValidation;

namespace SharedApi;

public static class PlatformApiExtensions
{
    public const string UrlSegmentPrefix = "api/v{version}";

    public static readonly ApiVersion DefaultApiVersion = new(1, 0);

    /// <summary>
    /// Registers the services every Wolverine HTTP endpoint on the platform depends on.
    /// </summary>
    public static IServiceCollection AddPlatformApi(this IServiceCollection services)
    {
        services.AddWolverineHttp();

        return services;
    }

    /// <summary>
    /// Maps the Wolverine HTTP endpoints of the calling service using the conventions
    /// shared by every service on the platform. Routes are versioned as
    /// <c>/api/v{major}/...</c> from the <c>[ApiVersion]</c> attribute on each endpoint.
    /// </summary>
    public static WebApplication MapPlatformApi(
        this WebApplication app,
        bool requireAuthorization = true,
        Action<WolverineHttpOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(app);

        app.MapWolverineEndpoints(options =>
        {
            // Validators and middleware routinely depend on scoped services such as the DbContext.
            options.ServiceProviderSource = ServiceProviderSource.FromHttpContextRequestServices;

            options.UseApiVersioning(versioning =>
            {
                versioning.UrlSegmentPrefix = UrlSegmentPrefix;
                versioning.DefaultVersion = DefaultApiVersion;
                versioning.EmitApiSupportedVersionsHeader = true;
                versioning.EmitDeprecationHeaders = true;
            });

            options.UseFluentValidationProblemDetailMiddleware();

            options.AddPolicy<SecurityHttpPolicy>();
            options.AddPolicy<OutputCacheHttpPolicy>();

            if (requireAuthorization)
            {
                options.RequireAuthorizeOnAll();
            }

            configure?.Invoke(options);
        });

        app.DescribeWolverineApiVersions();

        return app;
    }
}
