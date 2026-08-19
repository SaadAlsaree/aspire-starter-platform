namespace Simble.Features.SampleFeature.UpdateSample;

public sealed record UpdateSampleCommand(Guid Id, string Name, string? Description);
