using FluentValidation;

namespace Application.Features.Enrollment.Commands.EnrollTeacher;

public class EnrollTeacherCommandValidator : AbstractValidator<EnrollTeacherCommand>
{
    public EnrollTeacherCommandValidator()
    {
        RuleFor(x => x.SchoolId).NotEmpty();
        RuleFor(x => x.TeacherId).NotEmpty();
        RuleFor(x => x.ClassRoomId).NotEmpty();
        RuleFor(x => x.TermId).NotEmpty();
    }
}
