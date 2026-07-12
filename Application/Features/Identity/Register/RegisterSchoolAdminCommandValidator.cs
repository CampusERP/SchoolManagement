using FluentValidation;

namespace Application.Features.Identity.Register;

public class RegisterSchoolAdminCommandValidator : AbstractValidator<RegisterSchoolAdminCommand>
{
    public RegisterSchoolAdminCommandValidator()
    {
        RuleFor(x => x.SchoolId).NotEmpty();
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8)
            .WithMessage("Password must be at least 8 characters.");
    }
}