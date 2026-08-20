using Mapster;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using SharedInfrastructure.Collections;
using SharedInfrastructure.Linq;
using SharedKernel.Abstractions;
using SharedKernel.Results;
using Simble.Data;
using Simble.Data.Entities;

namespace Simble.Features.SampleFeature.GetListSample;

public sealed class GetListSampleHandler(AppDbContext db, IMapper mapper)
    : IQueryHandler<GetListSampleQuery, PagedList<GetListSampleViewModel>>
{
    public async Task<Result<PagedList<GetListSampleViewModel>>> ExecuteAsync(
        GetListSampleQuery query,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var samples = Filter(db.Samples.AsNoTracking(), query.SearchTerm)
            .ApplySort(query.SortBy, query.OrderBy, defaultSortBy: nameof(Sample.Name))
            .ProjectToType<GetListSampleViewModel>(mapper.Config);

        return await PagedList.CreateAsync(
            samples,
            query.Page,
            query.PageSize,
            cancellationToken);
    }

    private static IQueryable<Sample> Filter(IQueryable<Sample> samples, string? searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return samples;
        }

        var pattern = $"%{searchTerm.Trim()}%";

        return samples.Where(sample =>
            EF.Functions.ILike(sample.Name, pattern)
            || (sample.Description != null
                && EF.Functions.ILike(sample.Description, pattern)));
    }
}
