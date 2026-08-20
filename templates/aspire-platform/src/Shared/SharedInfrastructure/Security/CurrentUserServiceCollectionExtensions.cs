using Microsoft.Extensions.DependencyInjection;
using SharedKernel.Abstractions;

namespace SharedInfrastructure.Security;

public static class CurrentUserServiceCollectionExtensions
{
    public static IServiceCollection AddCurrentUser(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddSingleton<ICurrentUser, CurrentUser>();

        return services;
    }
}
