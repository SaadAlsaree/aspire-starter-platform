namespace Simble.Features.SampleFeature.GetByIdSample;

public sealed record GetByIdSampleViewModel(
    Guid Id,
    string Name,
    string? Description,
    DateTimeOffset CreatedAt,
    DateTimeOffset? LastUpdatedAt);
