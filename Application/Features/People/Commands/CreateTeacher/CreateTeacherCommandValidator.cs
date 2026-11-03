using FluentValidation;

namespace Application.Features.People.Commands.CreateTeacher;

public class CreateTeacherCommandValidator : AbstractValidator<CreateTeacherCommand>
{
    public CreateTeacherCommandValidator()
    {
        RuleFor(x => x.SchoolId).NotEmpty();
        RuleFor(x => x.EmployeeCode).NotEmpty().MaximumLength(50);
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8);
        RuleFor(x => x.Password).Matches("[A-Z]").WithMessage("Password must contain an uppercase letter.");
        RuleFor(x => x.Password).Matches("[a-z]").WithMessage("Password must contain a lowercase letter.");
        RuleFor(x => x.Password).Matches("[0-9]").WithMessage("Password must contain a number.");
        RuleFor(x => x.Password).Matches("[^a-zA-Z0-9]").WithMessage("Password must contain a symbol.");
    }
}
