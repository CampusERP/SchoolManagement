using Domain.Entities.Attendance;

namespace Application.Common.Interfaces.Repositories;

public interface IAttendanceSessionRepository
{
    Task<AttendanceSession?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<AttendanceSession?> GetByScheduleAndDateAsync(
        Guid schoolId, Guid classScheduleId, DateOnly date, CancellationToken ct = default);

    Task AddAsync(AttendanceSession session, CancellationToken ct = default);

    Task SaveChangesAsync(CancellationToken ct = default);
}
