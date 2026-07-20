using FluentValidation;

namespace Application.Features.Attendance.Commands.RecordAttendance;

public class RecordAttendanceCommandValidator : AbstractValidator<RecordAttendanceCommand>
{
    public RecordAttendanceCommandValidator()
    {
        RuleFor(x => x.SchoolId).NotEmpty();
        RuleFor(x => x.ClassScheduleId).NotEmpty();
        RuleFor(x => x.Date).NotEmpty();
        RuleFor(x => x.Entries).NotEmpty()
            .WithMessage("At least one student attendance entry is required.");
        RuleForEach(x => x.Entries).ChildRules(e =>
            e.RuleFor(s => s.StudentEnrollmentId).NotEmpty());
    }
}
