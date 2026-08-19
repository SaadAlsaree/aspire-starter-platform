using System.Reflection;
using JasperFx;
using JasperFx.CodeGeneration;
using JasperFx.CodeGeneration.Model;
using Microsoft.Extensions.Hosting;
using Wolverine;
using Wolverine.FluentValidation;

namespace SharedMessaging;

public static class WolverineDefaultsExtensions
{
    public static IHostBuilder AddWolverineDefaults(
        this IHostBuilder host,
        IHostEnvironment environment,
        Assembly applicationAssembly,
        Action<WolverineOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(host);
        ArgumentNullException.ThrowIfNull(environment);
        ArgumentNullException.ThrowIfNull(applicationAssembly);

        host.UseWolverine(options =>
        {
            options.ApplicationAssembly = applicationAssembly;
            options.ServiceName = applicationAssembly.GetName().Name
                ?? throw new InvalidOperationException(
                    "The application assembly must have a name.");
            options.Discovery.IncludeAssembly(applicationAssembly);
            options.CodeGeneration.TypeLoadMode = environment.IsDevelopment()
                ? TypeLoadMode.Auto
                : TypeLoadMode.Static;
            options.ServiceLocationPolicy = ServiceLocationPolicy.NotAllowed;

            options.UseFluentValidation();
            configure?.Invoke(options);
        });

        return host;
    }
}
