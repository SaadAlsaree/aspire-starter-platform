using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Simble.Data;

namespace Simble.Features.SampleFeature.CreateSample;

public sealed class CreateSampleValidator : AbstractValidator<CreateSampleCommand>
{
    private readonly AppDbContext _context;

    public CreateSampleValidator(AppDbContext context)
    {
        _context = context;

        RuleFor(command => command.Name)
            .NotEmpty()
            .MaximumLength(100)
            .MustAsync(BeUniqueName)
            .WithMessage("A sample with this name already exists.");

        RuleFor(command => command.Description)
            .MaximumLength(500);
    }

    private async Task<bool> BeUniqueName(string name, CancellationToken ct)
        => !await _context.Samples.AnyAsync(x => x.Name == name, ct);
}
