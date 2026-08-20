using Serilog;

namespace Simble.DependencyInjection;

public static class LoggingHostBuilderExtensions
{
    public static IHostBuilder AddApplicationLogging(this IHostBuilder host)
    {
        host.UseSerilog((context, configuration) =>
            configuration.ReadFrom.Configuration(context.Configuration));

        return host;
    }
}
