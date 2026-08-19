using SharedKernel.Abstractions;
using SharedKernel.Results;
using Simble.Data;
using Simble.Data.Entities;

namespace Simble.Features.SampleFeature.CreateSample;

public sealed class CreateSampleHandler(AppDbContext db)
    : ICommandHandler<CreateSampleCommand, Guid>
{
    public async Task<Result<Guid>> ExecuteAsync(
        CreateSampleCommand command,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var sample = new Sample
        {
            Name = command.Name,
            Description = command.Description,
        };

        db.Samples.Add(sample);
        await db.SaveChangesAsync(cancellationToken);

        return sample.Id;
    }
}
