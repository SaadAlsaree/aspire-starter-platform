using SharedKernel.Results;

namespace Simble.Features.SampleFeature;

internal static class SampleErrors
{
    public static DomainError NotFound(Guid id) =>
        DomainError.NotFound("Sample.NotFound", $"Sample '{id}' was not found.");

    public static DomainError NameAlreadyExists(string name) =>
        DomainError.Conflict("Sample.NameConflict", $"A sample named '{name}' already exists.");
}
