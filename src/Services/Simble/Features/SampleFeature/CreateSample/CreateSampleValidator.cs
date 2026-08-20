using FluentValidation;

namespace Simble.Features.SampleFeature.CreateSample;

public sealed class CreateSampleValidator : AbstractValidator<CreateSampleCommand>
{
    public CreateSampleValidator()
    {
        RuleFor(command => command.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(command => command.Description)
            .MaximumLength(500);
    }
}
