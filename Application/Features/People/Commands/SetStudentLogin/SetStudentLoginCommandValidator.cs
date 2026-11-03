using FluentValidation;

namespace Application.Features.People.Commands.SetStudentLogin;

public sealed class SetStudentLoginCommandValidator : AbstractValidator<SetStudentLoginCommand>
{
    public SetStudentLoginCommandValidator()
    {
        RuleFor(command => command.SchoolId).NotEmpty();
        RuleFor(command => command.StudentId).NotEmpty();
        When(command => !string.IsNullOrWhiteSpace(command.Email), () =>
            RuleFor(command => command.Email).EmailAddress().MaximumLength(256));
        When(command => !string.IsNullOrWhiteSpace(command.Password), () =>
        {
            RuleFor(command => command.Password!).MinimumLength(8)
                .WithMessage("Password must be at least 8 characters.");
            RuleFor(command => command.Password!).Matches("[A-Z]").WithMessage("Password must contain an uppercase letter.");
            RuleFor(command => command.Password!).Matches("[a-z]").WithMessage("Password must contain a lowercase letter.");
            RuleFor(command => command.Password!).Matches("[0-9]").WithMessage("Password must contain a number.");
            RuleFor(command => command.Password!).Matches("[^a-zA-Z0-9]").WithMessage("Password must contain a symbol.");
        });
    }
}
