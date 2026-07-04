using FluentValidation;

namespace Application.Features.Schools.Commands.CreateSchool;

// ── Validator ─────────────────────────────────────────────────────────
public class CreateSchoolCommandValidator : AbstractValidator<CreateSchoolCommand>
{
    public CreateSchoolCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("School name is required.")
            .MaximumLength(200);

        RuleFor(x => x.SubdomainCode)
            .NotEmpty().WithMessage("Subdomain code is required.")
            .MaximumLength(100)
            .Matches("^[a-z0-9-]+$").WithMessage("Subdomain code must be lowercase letters, numbers, and hyphens only.");
    }
}