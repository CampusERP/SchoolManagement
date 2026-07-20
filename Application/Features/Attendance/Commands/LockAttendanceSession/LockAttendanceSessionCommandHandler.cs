using Application.Common.Interfaces.Repositories;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Attendance.Commands.LockAttendanceSession;

public class LockAttendanceSessionCommandHandler
    : IRequestHandler<LockAttendanceSessionCommand, Result>
{
    private readonly IAttendanceSessionRepository _sessions;

    public LockAttendanceSessionCommandHandler(IAttendanceSessionRepository sessions)
    {
        _sessions = sessions;
    }

    public async Task<Result> Handle(LockAttendanceSessionCommand request, CancellationToken ct)
    {
        var session = await _sessions.GetByIdAsync(request.AttendanceSessionId, ct);
        if (session is null)
            return Result.Failure("Attendance session not found.");

        if (session.SchoolId != request.SchoolId)
            return Result.Failure("Attendance session does not belong to this school.");

        try
        {
            session.Lock();
            await _sessions.SaveChangesAsync(ct);
        }
        catch (Domain.Exceptions.DomainException ex)
        {
            return Result.Failure(ex.Message);
        }

        return Result.Success();
    }
}
