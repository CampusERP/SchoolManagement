using FluentValidation;

namespace Application.Features.People.Commands.CreateParent;

public class CreateParentCommandValidator : AbstractValidator<CreateParentCommand>
{
    public CreateParentCommandValidator()
    {
        RuleFor(x => x.SchoolId).NotEmpty();
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8);
    }
}