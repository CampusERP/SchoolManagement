using FluentValidation;

namespace Application.Features.Academics.UpdateEducationStage;

public class UpdateEducationStageCommandValidator : AbstractValidator<UpdateEducationStageCommand>
{
    public UpdateEducationStageCommandValidator()
    {
        RuleFor(x => x.EducationStageId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
    }
}
