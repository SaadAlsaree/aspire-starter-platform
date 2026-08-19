using Mapster;
using Simble.Data.Entities;
using Simble.Features.SampleFeature.GetByIdSample;
using Simble.Features.SampleFeature.GetListSample;

namespace Simble.Mappings;

public sealed class SampleMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        ArgumentNullException.ThrowIfNull(config);

        config.NewConfig<Sample, GetByIdSampleViewModel>();
        config.NewConfig<Sample, GetListSampleViewModel>();
    }
}
