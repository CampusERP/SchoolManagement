using FluentValidation;

namespace Application.Features.Subjects.Commands.CreateSubject;

public class AddCurriculumSubjectCommandValidator : AbstractValidator<AddCurriculumSubjectCommand>
{
    public AddCurriculumSubjectCommandValidator()
    {
        RuleFor(x => x.SchoolId).NotEmpty();
        RuleFor(x => x.GradeLevelId).NotEmpty();
        RuleFor(x => x.SubjectId).NotEmpty();
    }
}
