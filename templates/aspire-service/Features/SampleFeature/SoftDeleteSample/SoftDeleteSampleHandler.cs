using Microsoft.EntityFrameworkCore;
using SharedKernel.Abstractions;
using SharedKernel.Results;
using Simble.Data;

namespace Simble.Features.SampleFeature.SoftDeleteSample;

public sealed class SoftDeleteSampleHandler(AppDbContext db)
    : ICommandHandler<SoftDeleteSampleCommand>
{
    public async Task<Result> ExecuteAsync(
        SoftDeleteSampleCommand command,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var sample = await db.Samples
            .FirstOrDefaultAsync(sample => sample.Id == command.Id, cancellationToken);

        if (sample is null)
        {
            return Result.Failure(SampleErrors.NotFound(command.Id));
        }

        db.Samples.Remove(sample);
        await db.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
