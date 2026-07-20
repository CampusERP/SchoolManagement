using FluentValidation;

namespace Application.Features.Subjects.Commands.CreateSubject;

public class CreateSubjectCommandValidator : AbstractValidator<CreateSubjectCommand>
{
    public CreateSubjectCommandValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(20)
            .Matches("^[A-Za-z0-9]+$").WithMessage("Code must contain only letters and numbers.");
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
    }
}
