using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;
using SharedKernel.Abstractions;
using Simble.Data.Entities;

namespace Simble.UnitTests;

public class AuditSaveChangesInterceptorTests
{
    private static readonly DateTimeOffset Now = new(2026, 8, 18, 12, 0, 0, TimeSpan.Zero);
    private static readonly Guid UserId = Guid.CreateVersion7();

    [Fact]
    public async Task Insert_StampsCreationAudit()
    {
        using var factory = CreateFactory();
        await using var context = factory.CreateContext();

        var sample = new Sample { Name = "first" };
        context.Samples.Add(sample);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        Assert.Equal(Now, sample.CreatedAt);
        Assert.Equal(UserId, sample.CreatedBy);
        Assert.Null(sample.LastUpdatedAt);
    }

    [Fact]
    public async Task Update_StampsModificationAuditAndRotatesVersion()
    {
        using var factory = CreateFactory();
        await using var context = factory.CreateContext();

        var sample = new Sample { Name = "first" };
        context.Samples.Add(sample);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        var originalVersion = sample.Version;

        sample.Description = "changed";
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        Assert.Equal(Now, sample.LastUpdatedAt);
        Assert.Equal(UserId, sample.LastUpdatedBy);
        Assert.NotEqual(originalVersion, sample.Version);
    }

    [Fact]
    public async Task Remove_SoftDeletesAndHidesFromQueries()
    {
        using var factory = CreateFactory();
        await using var context = factory.CreateContext();

        var sample = new Sample { Name = "first" };
        context.Samples.Add(sample);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        context.Samples.Remove(sample);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        Assert.True(sample.IsDeleted);
        Assert.Equal(Now, sample.DeletedAt);
        Assert.Equal(UserId, sample.DeletedBy);

        await using var reader = factory.CreateContext();
        Assert.Empty(await reader.Samples.ToListAsync(TestContext.Current.CancellationToken));
        Assert.Single(await reader.Samples
            .IgnoreQueryFilters()
            .ToListAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task ConcurrentUpdate_ThrowsConcurrencyException()
    {
        using var factory = CreateFactory();

        await using (var seed = factory.CreateContext())
        {
            seed.Samples.Add(new Sample { Name = "first" });
            await seed.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        await using var first = factory.CreateContext();
        await using var second = factory.CreateContext();

        var fromFirst = await first.Samples.SingleAsync(TestContext.Current.CancellationToken);
        var fromSecond = await second.Samples.SingleAsync(TestContext.Current.CancellationToken);

        fromFirst.Description = "winner";
        await first.SaveChangesAsync(TestContext.Current.CancellationToken);

        fromSecond.Description = "loser";

        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() =>
            second.SaveChangesAsync(TestContext.Current.CancellationToken));
    }

    private static SampleDbContextFactory CreateFactory()
    {
        var currentUser = Substitute.For<ICurrentUser>();
        currentUser.Id.Returns(UserId);

        return SampleDbContextFactory.Create(new FakeTimeProvider(Now), currentUser);
    }
}
