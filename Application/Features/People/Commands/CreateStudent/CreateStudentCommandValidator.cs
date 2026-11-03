using FluentValidation;

namespace Application.Features.People.Commands.CreateStudent;

public class CreateStudentCommandValidator : AbstractValidator<CreateStudentCommand>
{
    public CreateStudentCommandValidator()
    {
        RuleFor(x => x.SchoolId).NotEmpty();
        RuleFor(x => x.StudentCode).MaximumLength(50);
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.SecondName).MaximumLength(100);
        RuleFor(x => x.ThirdName).MaximumLength(100);
        RuleFor(x => x.NationalityCountry).MaximumLength(100);
        RuleFor(x => x.ResidenceCountry).MaximumLength(100);
        RuleFor(x => x.DateOfBirth).NotEmpty().LessThan(DateTime.Today)
            .WithMessage("Date of birth must be in the past.");
        When(x => x.NationalId != null, () => RuleFor(x => x.NationalId).MaximumLength(50));
        When(x => !string.IsNullOrWhiteSpace(x.Email) || !string.IsNullOrWhiteSpace(x.Password), () =>
        {
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
            RuleFor(x => x.Password).NotEmpty().MinimumLength(8)
                .WithMessage("Password must be at least 8 characters.");
            RuleFor(x => x.Password!).Matches("[A-Z]").WithMessage("Password must contain an uppercase letter.");
            RuleFor(x => x.Password!).Matches("[a-z]").WithMessage("Password must contain a lowercase letter.");
            RuleFor(x => x.Password!).Matches("[0-9]").WithMessage("Password must contain a number.");
            RuleFor(x => x.Password!).Matches("[^a-zA-Z0-9]").WithMessage("Password must contain a symbol.");
        });
    }
}
