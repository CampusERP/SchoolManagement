using FluentValidation;

namespace Application.Features.People.Commands.LinkStudentGuardian;

public class LinkStudentGuardianCommandValidator : AbstractValidator<LinkStudentGuardianCommand>
{
    public LinkStudentGuardianCommandValidator()
    {
        RuleFor(x => x.SchoolId).NotEmpty();
        RuleFor(x => x.StudentId).NotEmpty();
        RuleFor(x => x.ParentId).NotEmpty();
    }
}