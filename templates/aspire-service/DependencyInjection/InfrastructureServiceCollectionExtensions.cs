using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using SharedInfrastructure.Persistence;
using SharedInfrastructure.Security;
using Simble.Data;

namespace Simble.DependencyInjection;

public static class InfrastructureServiceCollectionExtensions
{
    public const string DatabaseConnectionName = "simbledb";

    public static IHostApplicationBuilder AddInfrastructure(this IHostApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Services.AddSingleton(TimeProvider.System);
        builder.Services.AddCurrentUser();
        builder.Services.AddSingleton<AuditSaveChangesInterceptor>();

        builder.Services
            .AddOptions<DatabaseMigrationOptions>()
            .Bind(builder.Configuration.GetSection(DatabaseMigrationOptions.SectionName));

        builder.Services.AddDbContextPool<AppDbContext>((serviceProvider, options) =>
            options
                .UseNpgsql(builder.Configuration.GetConnectionString(DatabaseConnectionName))
                .AddInterceptors(
                    serviceProvider.GetRequiredService<AuditSaveChangesInterceptor>()));

        builder.EnrichNpgsqlDbContext<AppDbContext>();

        return builder;
    }
}
