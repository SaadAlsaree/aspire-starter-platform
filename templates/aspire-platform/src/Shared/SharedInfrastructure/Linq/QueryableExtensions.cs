using System.Linq.Dynamic.Core;
using System.Reflection;

namespace SharedInfrastructure.Linq;

public static class QueryableExtensions
{
    private const string DefaultProperty = "Id";
    private const string Ascending = "ascending";
    private const string Descending = "descending";

    public static IQueryable<T> ApplySort<T>(
        this IQueryable<T> query,
        string? sortBy,
        string? orderBy,
        string defaultSortBy = DefaultProperty,
        string tieBreaker = DefaultProperty)
    {
        ArgumentNullException.ThrowIfNull(query);

        var property = ResolveProperty<T>(sortBy) ?? ResolveProperty<T>(defaultSortBy);
        if (property is null)
        {
            return query;
        }

        var direction = string.Equals(orderBy, "desc", StringComparison.OrdinalIgnoreCase)
            ? Descending
            : Ascending;

        var tieBreakerProperty = ResolveProperty<T>(tieBreaker);
        var ordering = tieBreakerProperty is null
            || string.Equals(tieBreakerProperty, property, StringComparison.Ordinal)
            ? $"{property} {direction}"
            : $"{property} {direction}, {tieBreakerProperty} {Ascending}";

        return query.OrderBy(ordering);
    }

    private static string? ResolveProperty<T>(string? name) =>
        string.IsNullOrWhiteSpace(name)
            ? null
            : typeof(T).GetProperty(
                name,
                BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance)?.Name;
}
