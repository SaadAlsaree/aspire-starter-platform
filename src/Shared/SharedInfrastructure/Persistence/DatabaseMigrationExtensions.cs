using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace SharedInfrastructure.Persistence;

public static partial class DatabaseMigrationExtensions
{
    public static async Task MigrateDatabaseAsync<TContext>(
        this IServiceProvider services,
        CancellationToken cancellationToken = default)
        where TContext : DbContext
    {
        ArgumentNullException.ThrowIfNull(services);

        await using var scope = services.CreateAsyncScope();
        var provider = scope.ServiceProvider;
        var logger = provider.GetRequiredService<ILoggerFactory>()
            .CreateLogger(typeof(DatabaseMigrationExtensions));
        var contextName = typeof(TContext).Name;

        var options = provider.GetService<IOptions<DatabaseMigrationOptions>>()?.Value
            ?? new DatabaseMigrationOptions();

        if (!options.RunOnStartup)
        {
            LogMigrationsSkipped(logger, contextName);
            return;
        }

        var context = provider.GetRequiredService<TContext>();
        var strategy = context.Database.CreateExecutionStrategy();

        await strategy.ExecuteAsync(async () =>
        {
            var pending = (await context.Database
                .GetPendingMigrationsAsync(cancellationToken)).ToArray();

            if (pending.Length == 0)
            {
                LogDatabaseUpToDate(logger, contextName);
                return;
            }

            LogApplyingMigrations(logger, pending.Length, contextName, pending);
            await context.Database.MigrateAsync(cancellationToken);
            LogMigrationsCompleted(logger, contextName);
        });
    }

    [LoggerMessage(
        EventId = 2000,
        Level = LogLevel.Information,
        Message = "Startup migrations are disabled for {DbContext}")]
    private static partial void LogMigrationsSkipped(ILogger logger, string dbContext);

    [LoggerMessage(
        EventId = 2001,
        Level = LogLevel.Information,
        Message = "Database for {DbContext} is already up to date")]
    private static partial void LogDatabaseUpToDate(ILogger logger, string dbContext);

    [LoggerMessage(
        EventId = 2002,
        Level = LogLevel.Information,
        Message = "Applying {MigrationCount} migration(s) for {DbContext}: {Migrations}")]
    private static partial void LogApplyingMigrations(
        ILogger logger,
        int migrationCount,
        string dbContext,
        string[] migrations);

    [LoggerMessage(
        EventId = 2003,
        Level = LogLevel.Information,
        Message = "Database migrations completed for {DbContext}")]
    private static partial void LogMigrationsCompleted(ILogger logger, string dbContext);
}
