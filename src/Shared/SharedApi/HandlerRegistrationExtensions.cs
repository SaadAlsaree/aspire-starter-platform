using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using SharedKernel.Abstractions;

namespace SharedApi;

public static class HandlerRegistrationExtensions
{
    /// <summary>
    /// Registers every <see cref="IHandler"/> in the assembly as a scoped service so
    /// endpoints can take them as constructor or method parameters.
    /// </summary>
    public static IServiceCollection AddHandlers(
        this IServiceCollection services,
        Assembly applicationAssembly)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(applicationAssembly);

        var handlerTypes = applicationAssembly.DefinedTypes
            .Where(type => type is { IsAbstract: false, IsInterface: false }
                && typeof(IHandler).IsAssignableFrom(type));

        foreach (var handlerType in handlerTypes)
        {
            services.AddScoped(handlerType);

            foreach (var contract in handlerType.ImplementedInterfaces
                .Where(contract => contract != typeof(IHandler)
                    && typeof(IHandler).IsAssignableFrom(contract)))
            {
                services.AddScoped(contract, provider => provider.GetRequiredService(handlerType));
            }
        }

        return services;
    }
}
