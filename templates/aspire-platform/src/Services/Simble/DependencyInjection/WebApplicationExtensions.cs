using App.ServiceDefaults;
using App.ServiceDefaults.Authentication;
using Microsoft.Extensions.Hosting;
using Serilog;
using SharedApi;

namespace Simble.DependencyInjection;

public static class WebApplicationExtensions
{
    public static WebApplication UseApplicationPipeline(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        app.UseForwardedHeaders();
        app.UseSerilogRequestLogging();
        app.UseExceptionHandler();
        app.UseStatusCodePages();
        app.UseCors(ApiServiceCollectionExtensions.DefaultCorsPolicy);

        // AuthenticationMiddleware needs a registered scheme, so it is only added when a
        // provider is configured.
        var authenticationEnabled =
            app.Configuration.GetAuthenticationProvider() != AuthenticationProvider.None;

        if (authenticationEnabled)
        {
            app.UseAuthentication();
        }

        app.UseAuthorization();
        app.UseRateLimiter();
        app.UseOutputCache();

        app.MapDefaultEndpoints();
        app.MapDefaultOpenApi();
        app.MapPlatformApi(requireAuthorization: authenticationEnabled);

        return app;
    }
}
