using SharedInfrastructure.Linq;

namespace Simble.UnitTests;

public class QueryableExtensionsTests
{
    private static readonly Row[] Rows =
    [
        new(Guid.Parse("00000000-0000-0000-0000-000000000003"), "b"),
        new(Guid.Parse("00000000-0000-0000-0000-000000000001"), "a"),
        new(Guid.Parse("00000000-0000-0000-0000-000000000002"), "b"),
    ];

    [Fact]
    public void ApplySort_SortsAscendingByRequestedProperty()
    {
        var sorted = Rows.AsQueryable().ApplySort("Name", "asc").ToArray();

        Assert.Equal(["a", "b", "b"], sorted.Select(row => row.Name));
    }

    [Fact]
    public void ApplySort_SortsDescending()
    {
        var sorted = Rows.AsQueryable().ApplySort("Name", "desc").ToArray();

        Assert.Equal(["b", "b", "a"], sorted.Select(row => row.Name));
    }

    [Fact]
    public void ApplySort_AppliesTieBreakerForStablePaging()
    {
        var sorted = Rows.AsQueryable().ApplySort("Name", "asc").ToArray();

        Assert.Equal(
            [Rows[1].Id, Rows[2].Id, Rows[0].Id],
            sorted.Select(row => row.Id));
    }

    [Fact]
    public void ApplySort_IsCaseInsensitive()
    {
        var sorted = Rows.AsQueryable().ApplySort("name", "asc").ToArray();

        Assert.Equal("a", sorted[0].Name);
    }

    [Fact]
    public void ApplySort_FallsBackToDefaultWhenPropertyIsUnknown()
    {
        var sorted = Rows.AsQueryable()
            .ApplySort("DROP TABLE samples", "asc", defaultSortBy: "Name")
            .ToArray();

        Assert.Equal("a", sorted[0].Name);
    }

    private sealed record Row(Guid Id, string Name);
}
