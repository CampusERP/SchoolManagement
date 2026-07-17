using FluentValidation;

namespace Application.Features.Enrollment.Commands.AssignTeacher;

public class AssignTeacherCommandValidator : AbstractValidator<AssignTeacherCommand>
{
    public AssignTeacherCommandValidator()
    {
        RuleFor(x => x.SchoolId).NotEmpty();
        RuleFor(x => x.TeacherId).NotEmpty();
        RuleFor(x => x.SubjectId).NotEmpty();
        RuleFor(x => x.ClassRoomId).NotEmpty();
        RuleFor(x => x.TermId).NotEmpty();
        RuleFor(x => x.ScheduleSlots).NotEmpty()
            .WithMessage("At least one schedule slot is required.");
        RuleForEach(x => x.ScheduleSlots).ChildRules(slot =>
        {
            slot.RuleFor(s => s.RoomId).NotEmpty();
            slot.RuleFor(s => s.EndTime).GreaterThan(s => s.StartTime)
                .WithMessage("Schedule end time must be after start time.");
        });
    }
}
