using FluentValidation;

namespace Application.Features.People.Commands.CreateStudent;

public class CreateStudentCommandValidator : AbstractValidator<CreateStudentCommand>
{
    public CreateStudentCommandValidator()
    {
        RuleFor(x => x.SchoolId).NotEmpty();
        RuleFor(x => x.StudentCode).NotEmpty().MaximumLength(50);
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.DateOfBirth).NotEmpty().LessThan(DateTime.Today)
            .WithMessage("Date of birth must be in the past.");
        When(x => x.Email != null, () => RuleFor(x => x.Email).EmailAddress());
    }
}