using FluentValidation;

namespace Application.Features.Academics.DeleteEducationStage;

public class DeleteEducationStageCommandValidator : AbstractValidator<DeleteEducationStageCommand>
{
    public DeleteEducationStageCommandValidator()
    {
        RuleFor(x => x.EducationStageId).NotEmpty();
    }
}
