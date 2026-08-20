using Mapster;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Abstractions;
using SharedKernel.Results;
using Simble.Data;

namespace Simble.Features.SampleFeature.GetByIdSample;

public sealed class GetByIdSampleHandler(AppDbContext db, IMapper mapper)
    : IQueryHandler<GetByIdSampleQuery, GetByIdSampleViewModel>
{
    public async Task<Result<GetByIdSampleViewModel>> ExecuteAsync(
        GetByIdSampleQuery query,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var sample = await db.Samples
            .AsNoTracking()
            .Where(sample => sample.Id == query.Id)
            .ProjectToType<GetByIdSampleViewModel>(mapper.Config)
            .FirstOrDefaultAsync(cancellationToken);

        return sample is null ? SampleErrors.NotFound(query.Id) : sample;
    }
}
