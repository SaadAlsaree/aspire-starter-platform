using SharedKernel.Entities;

namespace Simble.Data.Entities;

public sealed class Sample : BaseEntity
{
    public required string Name { get; set; }

    public string? Description { get; set; }
}
