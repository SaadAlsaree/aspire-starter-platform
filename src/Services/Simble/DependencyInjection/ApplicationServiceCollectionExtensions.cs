using Mapster;
using MapsterMapper;
using Microsoft.Extensions.Hosting;
using SharedApi;
using SharedMessaging;
using Simble.Data;
using Wolverine;

namespace Simble.DependencyInjection;

public static class ApplicationServiceCollectionExtensions
{
    public static IHostBuilder AddApplicationMessaging(
        this IHostBuilder host,
        IHostEnvironment environment)
    {
        host.AddWolverineDefaults(environment, typeof(Program).Assembly, options =>
        {
            options.CodeGeneration.AlwaysUseServiceLocationFor<AppDbContext>();
            options.CodeGeneration.AlwaysUseServiceLocationFor<IMapper>();
            options.Policies.AutoApplyTransactions();
        });

        return host;
    }

    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddHandlers(typeof(Program).Assembly);

        var mapsterConfig = TypeAdapterConfig.GlobalSettings;
        mapsterConfig.Scan(typeof(Program).Assembly);
        services.AddSingleton(mapsterConfig);
        services.AddScoped<IMapper, ServiceMapper>();

        return services;
    }
}
