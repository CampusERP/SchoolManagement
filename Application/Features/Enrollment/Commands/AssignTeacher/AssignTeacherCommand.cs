using Application.Common.Behaviors;
using Application.Common.Interfaces;

namespace Application.Features.Enrollment.Commands.AssignTeacher;

public record AssignTeacherCommand(
    Guid SchoolId,
    Guid TeacherId,
    Guid SubjectId,
    Guid ClassRoomId,
    Guid TermId,
    List<ScheduleSlot> ScheduleSlots) : ICommand<Guid>, IBaseCommand, ITenantScopedRequest;
