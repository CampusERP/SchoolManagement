using FluentValidation;

namespace Application.Features.People.Commands.LinkStudentGuardian;

public class LinkStudentGuardianCommandValidator : AbstractValidator<LinkStudentGuardianCommand>
{
    public LinkStudentGuardianCommandValidator()
    {
        RuleFor(x => x.StudentId)
            .NotEmpty().WithMessage("Student ID is required.");

        RuleFor(x => x.ParentId)
            .NotEmpty().WithMessage("Parent ID is required.");

        RuleFor(x => x.RelationshipType)
            .IsInEnum().WithMessage("Valid relationship type is required.");
    }
}
