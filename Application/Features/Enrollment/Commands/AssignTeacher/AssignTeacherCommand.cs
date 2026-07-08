using Application.Common.Behaviors;

namespace Application.Features.Enrollment.Commands.AssignTeacher;

public record AssignTeacherCommand(
    Guid TeacherId,
    Guid SubjectId,
    Guid ClassRoomId,
    Guid TermId) : ICommand<Guid>, IBaseCommand;
