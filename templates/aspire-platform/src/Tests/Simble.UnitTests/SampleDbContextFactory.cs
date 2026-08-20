using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Abstractions;
using Simble.Data;

namespace Simble.UnitTests;

internal sealed class SampleDbContextFactory : IDisposable
{
    private readonly SqliteConnection connection;
    private readonly AuditSaveChangesInterceptor interceptor;

    private SampleDbContextFactory(
        SqliteConnection connection,
        AuditSaveChangesInterceptor interceptor)
    {
        this.connection = connection;
        this.interceptor = interceptor;
    }

    public static SampleDbContextFactory Create(
        TimeProvider timeProvider,
        ICurrentUser currentUser)
    {
        var connection = new SqliteConnection("Filename=:memory:");
        connection.Open();

        var factory = new SampleDbContextFactory(
            connection,
            new AuditSaveChangesInterceptor(timeProvider, currentUser));

        using var context = factory.CreateContext();
        context.Database.EnsureCreated();

        return factory;
    }

    public AppDbContext CreateContext() =>
        new(new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .AddInterceptors(interceptor)
            .Options);

    public void Dispose() => connection.Dispose();
}
