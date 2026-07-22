using Application.Common.Behaviors;
using Application.Common.Interfaces;

namespace Application.Features.Enrollment.Commands.EnrollTeacher;

public record EnrollTeacherCommand(
    Guid SchoolId,
    Guid TeacherId,
    Guid ClassRoomId,
    Guid TermId) : ICommand<Guid>, IBaseCommand, ITenantScopedRequest;
