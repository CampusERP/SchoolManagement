using FluentValidation;

namespace Application.Features.Academics.CreateEducationStage;

public class CreateEducationStageCommandValidator : AbstractValidator<CreateEducationStageCommand>
{
    public CreateEducationStageCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
    }
}
