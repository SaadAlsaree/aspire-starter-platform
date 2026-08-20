using Microsoft.EntityFrameworkCore;
using SharedKernel.Abstractions;
using SharedKernel.Results;
using Simble.Data;

namespace Simble.Features.SampleFeature.UpdateSample;

public sealed class UpdateSampleHandler(AppDbContext db)
    : ICommandHandler<UpdateSampleCommand>
{
    public async Task<Result> ExecuteAsync(
        UpdateSampleCommand command,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var sample = await db.Samples
            .FirstOrDefaultAsync(sample => sample.Id == command.Id, cancellationToken);

        if (sample is null)
        {
            return Result.Failure(SampleErrors.NotFound(command.Id));
        }

        var nameTaken = await db.Samples.AnyAsync(
            other => other.Name == command.Name && other.Id != command.Id,
            cancellationToken);

        if (nameTaken)
        {
            return Result.Failure(SampleErrors.NameAlreadyExists(command.Name));
        }

        sample.Name = command.Name;
        sample.Description = command.Description;

        await db.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
