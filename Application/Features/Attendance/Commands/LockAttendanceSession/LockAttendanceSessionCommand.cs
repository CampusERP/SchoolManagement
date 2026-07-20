using Application.Common.Behaviors;
using Application.Common.Interfaces;

namespace Application.Features.Attendance.Commands.LockAttendanceSession;

public record LockAttendanceSessionCommand(
    Guid SchoolId,
    Guid AttendanceSessionId) : ICommand, IBaseCommand, ITenantScopedRequest;
