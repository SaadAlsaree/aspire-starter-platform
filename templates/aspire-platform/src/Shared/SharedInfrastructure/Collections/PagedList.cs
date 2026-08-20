using Microsoft.EntityFrameworkCore;

namespace SharedInfrastructure.Collections;

public sealed record PagedList<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    int TotalCount)
{
    public int TotalPages =>
        PageSize > 0 ? (int)Math.Ceiling(TotalCount / (double)PageSize) : 0;

    public bool HasPreviousPage => Page > 1;

    public bool HasNextPage => Page < TotalPages;
}

public static class PagedList
{
    public const int DefaultPageSize = 20;
    public const int MaxPageSize = 200;

    public static async Task<PagedList<T>> CreateAsync<T>(
        IQueryable<T> source,
        int? page,
        int? pageSize,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);

        var normalizedPage = Math.Max(page ?? 1, 1);
        var normalizedPageSize = Math.Clamp(pageSize ?? DefaultPageSize, 1, MaxPageSize);

        var totalCount = await source.CountAsync(cancellationToken);
        var items = await source
            .Skip((normalizedPage - 1) * normalizedPageSize)
            .Take(normalizedPageSize)
            .ToListAsync(cancellationToken);

        return new PagedList<T>(items, normalizedPage, normalizedPageSize, totalCount);
    }
}
