using FluentValidation;

namespace Application.Features.Subjects.Commands.RemoveCurriculumSubject;

public sealed class RemoveCurriculumSubjectCommandValidator : AbstractValidator<RemoveCurriculumSubjectCommand>
{
    public RemoveCurriculumSubjectCommandValidator()
    {
        RuleFor(x => x.SchoolId).NotEmpty();
        RuleFor(x => x.CurriculumSubjectId).NotEmpty();
    }
}
