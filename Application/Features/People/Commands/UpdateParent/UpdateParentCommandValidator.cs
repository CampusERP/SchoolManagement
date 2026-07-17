using FluentValidation;

namespace Application.Features.People.Commands.UpdateParent;

public class UpdateParentCommandValidator : AbstractValidator<UpdateParentCommand>
{
    public UpdateParentCommandValidator()
    {
        RuleFor(x => x.SchoolId).NotEmpty();
        RuleFor(x => x.ParentId).NotEmpty();
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
    }
}
