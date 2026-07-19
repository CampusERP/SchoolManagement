using FluentValidation;

namespace Application.Features.Exams.Commands;

public class AddExamScheduleCommandValidator : AbstractValidator<AddExamScheduleCommand>
{
    public AddExamScheduleCommandValidator()
    {
        RuleFor(x => x.ExamId).NotEmpty();
        RuleFor(x => x.ClassRoomId).NotEmpty();
        RuleFor(x => x.RoomId).NotEmpty();
        RuleFor(x => x.ExamDate).GreaterThan(DateTime.UtcNow.AddHours(-1));
    }
}
