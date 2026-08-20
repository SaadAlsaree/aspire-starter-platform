using SharedInfrastructure.Collections;

namespace Simble.UnitTests;

public class PagedListTests
{
    [Theory]
    [InlineData(0, 20, 0)]
    [InlineData(1, 20, 1)]
    [InlineData(20, 20, 1)]
    [InlineData(21, 20, 2)]
    public void TotalPages_RoundsUp(int totalCount, int pageSize, int expected)
    {
        var page = new PagedList<int>([], 1, pageSize, totalCount);

        Assert.Equal(expected, page.TotalPages);
    }

    [Fact]
    public void Navigation_ReflectsPosition()
    {
        var page = new PagedList<int>([1], 2, 10, 30);

        Assert.True(page.HasPreviousPage);
        Assert.True(page.HasNextPage);
    }

    [Fact]
    public void Navigation_OnLastPage_HasNoNext()
    {
        var page = new PagedList<int>([1], 3, 10, 30);

        Assert.True(page.HasPreviousPage);
        Assert.False(page.HasNextPage);
    }
}
