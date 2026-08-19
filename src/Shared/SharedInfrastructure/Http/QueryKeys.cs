namespace SharedInfrastructure.Http;

public static class QueryKeys
{
    public const string Page = "page";
    public const string PageSize = "pageSize";
    public const string SearchTerm = "searchTerm";
    public const string SortBy = "sortBy";
    public const string OrderBy = "orderBy";

    public static string[] Listing { get; } =
        [Page, PageSize, SearchTerm, SortBy, OrderBy];
}
