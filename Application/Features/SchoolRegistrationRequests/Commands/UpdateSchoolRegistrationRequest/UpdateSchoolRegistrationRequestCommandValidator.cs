using FluentValidation;

namespace Application.Features.SchoolRegistrationRequests.Commands.UpdateSchoolRegistrationRequest;

public class UpdateSchoolRegistrationRequestCommandValidator
    : AbstractValidator<UpdateSchoolRegistrationRequestCommand>
{
    public UpdateSchoolRegistrationRequestCommandValidator()
    {
        RuleFor(x => x.SchoolName)
            .NotEmpty().WithMessage("School name is required.")
            .MaximumLength(200);

        RuleFor(x => x.SubdomainCode)
            .NotEmpty().WithMessage("Subdomain code is required.")
            .MaximumLength(100)
            .Matches("^[a-z0-9-]+$")
            .WithMessage("Subdomain code must be lowercase letters, numbers, and hyphens only.");

        RuleFor(x => x.Address).MaximumLength(500);
        RuleFor(x => x.Phone).MaximumLength(50);

        RuleFor(x => x.SchoolEmail)
            .EmailAddress().WithMessage("Invalid school email address.")
            .When(x => !string.IsNullOrWhiteSpace(x.SchoolEmail));

        RuleFor(x => x.AdminFirstName)
            .NotEmpty().WithMessage("Admin first name is required.")
            .MaximumLength(100);

        RuleFor(x => x.AdminLastName)
            .NotEmpty().WithMessage("Admin last name is required.")
            .MaximumLength(100);

        RuleFor(x => x.AdminEmail)
            .NotEmpty().WithMessage("Admin email is required.")
            .EmailAddress().WithMessage("Invalid admin email address.")
            .MaximumLength(256);

        RuleFor(x => x.AdminPassword)
            .NotEmpty().WithMessage("Admin password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters.")
            .Matches("[A-Z]").WithMessage("Password must contain an uppercase letter.")
            .Matches("[a-z]").WithMessage("Password must contain a lowercase letter.")
            .Matches("[0-9]").WithMessage("Password must contain a digit.")
            .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain a non-alphanumeric character.")
            .When(x => !string.IsNullOrWhiteSpace(x.AdminPassword));

        RuleFor(x => x.ReviewNotes).MaximumLength(1000);
    }
}
