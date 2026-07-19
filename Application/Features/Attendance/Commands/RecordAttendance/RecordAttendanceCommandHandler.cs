using MediatR;
using Application.Common.Models;
using Domain.Entities.Attendance;
using Application.Common.Interfaces.Repositories;

namespace Application.Features.Attendance.Commands.RecordAttendance;

public class RecordAttendanceCommandHandler : IRequestHandler<RecordAttendanceCommand, Result<Guid>>
{
    private readonly IAttendanceSessionRepository _sessions;

    public RecordAttendanceCommandHandler(IAttendanceSessionRepository sessions)
    {
        _sessions = sessions;
    }

    public async Task<Result<Guid>> Handle(RecordAttendanceCommand request, CancellationToken ct)
    {
        if (request.Date > DateOnly.FromDateTime(DateTime.UtcNow))
            return Result.Failure<Guid>("Cannot record attendance for a future date.");

        var session = await _sessions.GetByScheduleAndDateAsync(
            request.SchoolId, request.ClassScheduleId, request.Date, ct);

        bool isNew = session is null;
        session ??= AttendanceSession.Open(request.SchoolId, request.ClassScheduleId, request.Date);

        if (session.IsLocked)
            return Result.Failure<Guid>("This attendance session is locked and cannot be modified.");

        foreach (var entry in request.Entries)
        {
            session.RecordStudent(entry.StudentEnrollmentId, entry.Status);
            if (!string.IsNullOrWhiteSpace(entry.Note))
            {
                var record = session.Records.First(r => r.StudentEnrollmentId == entry.StudentEnrollmentId);
                record.AddNote(entry.Note);
            }
        }

        if (isNew)
            await _sessions.AddAsync(session, ct);

        return Result.Success(session.Id);
    }
}
