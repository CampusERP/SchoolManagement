using FluentValidation;

namespace Application.Features.Academics.CreateGradeLevel;

public class CreateGradeLevelCommandValidator : AbstractValidator<CreateGradeLevelCommand>
{
    public CreateGradeLevelCommandValidator()
    {
        RuleFor(x => x.EducationStageId)
            .NotEmpty().WithMessage("EducationStage ID is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(100);

        RuleFor(x => x.Sequence)
            .GreaterThan(0).WithMessage("Sequence must be greater than zero.");
    }
}
