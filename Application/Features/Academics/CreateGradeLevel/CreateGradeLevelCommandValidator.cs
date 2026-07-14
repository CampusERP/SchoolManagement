using FluentValidation;

namespace Application.Features.Academics.CreateGradeLevel;

public class CreateGradeLevelCommandValidator : AbstractValidator<CreateGradeLevelCommand>
{
    public CreateGradeLevelCommandValidator()
    {
        RuleFor(x => x.SchoolId).NotEmpty();
        RuleFor(x => x.EducationStageId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Sequence).GreaterThan(0);
    }
}