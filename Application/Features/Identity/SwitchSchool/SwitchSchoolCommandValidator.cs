using FluentValidation;

namespace Application.Features.Identity.SwitchSchool;

public class SwitchSchoolCommandValidator : AbstractValidator<SwitchSchoolCommand>
{
    public SwitchSchoolCommandValidator()
    {
        RuleFor(x => x.TargetSchoolId).NotEmpty();
    }
}
