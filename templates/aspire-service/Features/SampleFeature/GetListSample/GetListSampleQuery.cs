namespace Simble.Features.SampleFeature.GetListSample;

public sealed record GetListSampleQuery(
    int? Page = null,
    int? PageSize = null,
    string? SearchTerm = null,
    string? SortBy = null,
    string? OrderBy = null);
