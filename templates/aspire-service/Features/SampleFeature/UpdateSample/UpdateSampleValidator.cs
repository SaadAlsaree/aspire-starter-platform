using FluentValidation;

namespace Simble.Features.SampleFeature.UpdateSample;

public sealed class UpdateSampleValidator : AbstractValidator<UpdateSampleRequest>
{
    public UpdateSampleValidator()
    {
        RuleFor(request => request.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(request => request.Description)
            .MaximumLength(500);
    }
}
