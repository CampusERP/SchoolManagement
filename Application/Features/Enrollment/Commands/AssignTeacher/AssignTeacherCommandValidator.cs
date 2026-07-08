using FluentValidation;

namespace Application.Features.Enrollment.Commands.AssignTeacher;

public class AssignTeacherCommandValidator : AbstractValidator<AssignTeacherCommand>
{
    public AssignTeacherCommandValidator()
    {
        RuleFor(x => x.TeacherId)
            .NotEmpty().WithMessage("Teacher ID is required.");

        RuleFor(x => x.SubjectId)
            .NotEmpty().WithMessage("Subject ID is required.");

        RuleFor(x => x.ClassRoomId)
            .NotEmpty().WithMessage("ClassRoom ID is required.");

        RuleFor(x => x.TermId)
            .NotEmpty().WithMessage("Term ID is required.");
    }
}
