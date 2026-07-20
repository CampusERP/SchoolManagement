using Application.Common.Interfaces.Repositories;
using Domain.Entities.Attendance;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class AttendanceSessionRepository : IAttendanceSessionRepository
{
    private readonly ApplicationDbContext _db;
    public AttendanceSessionRepository(ApplicationDbContext db) => _db = db;

    public async Task<AttendanceSession?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await _db.AttendanceSessions.FindAsync(new object[] { id }, ct);

    public async Task<AttendanceSession?> GetByScheduleAndDateAsync(
        Guid schoolId, Guid classScheduleId, DateOnly date, CancellationToken ct = default) =>
        await _db.AttendanceSessions.FirstOrDefaultAsync(
            s => s.SchoolId == schoolId && s.ClassScheduleId == classScheduleId && s.Date == date, ct);

    public async Task AddAsync(AttendanceSession session, CancellationToken ct = default) =>
        await _db.AttendanceSessions.AddAsync(session, ct);

    public async Task SaveChangesAsync(CancellationToken ct = default) =>
        await _db.SaveChangesAsync(ct);
}
