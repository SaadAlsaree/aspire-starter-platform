namespace Simble.Features.SampleFeature.GetListSample;

public sealed record GetListSampleViewModel(
    Guid Id,
    string Name,
    string? Description,
    DateTimeOffset CreatedAt);
