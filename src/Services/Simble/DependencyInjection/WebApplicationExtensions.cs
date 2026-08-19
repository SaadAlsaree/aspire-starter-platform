using App.ServiceDefaults;
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
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseRateLimiter();
        app.UseOutputCache();

        app.MapDefaultEndpoints();
        app.MapDefaultOpenApi();
        app.MapPlatformApi();

        return app;
    }
}
