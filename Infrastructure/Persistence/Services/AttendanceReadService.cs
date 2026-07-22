using Application.Common.Interfaces.Services;
using Application.Common.Models;
using Application.Features.Attendance.Queries.GetClassAttendance;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Services;

public class AttendanceReadService : IAttendanceReadService
{
    private readonly ApplicationDbContext _db;

    public AttendanceReadService(ApplicationDbContext db) => _db = db;

    public async Task<ClassAttendanceDto?> GetClassAttendanceAsync(
        Guid classScheduleId, DateOnly date, CancellationToken ct = default)
    {
        var classRoomId = await _db.ClassSchedules.AsNoTracking()
            .Where(cs => cs.Id == classScheduleId)
            .Join(_db.TeachingAssignments,
                cs => cs.TeachingAssignmentId, ta => ta.Id,
                (cs, ta) => ta.ClassRoomId)
            .FirstOrDefaultAsync(ct);

        if (classRoomId == Guid.Empty)
            return null;

        var enrollmentQuery = _db.StudentEnrollments.AsNoTracking()
            .Where(e => e.ClassRoomId == classRoomId && e.Status == EnrollmentStatus.Active);

        var session = await _db.AttendanceSessions.AsNoTracking()
            .Where(s => s.ClassScheduleId == classScheduleId && s.Date == date)
            .FirstOrDefaultAsync(ct);

        if (session is null)
        {
            var students = await enrollmentQuery
                .Join(_db.Students,
                    e => e.StudentId, s => s.Id,
                    (e, s) => new StudentAttendanceDto(
                        e.Id,
                        s.FirstName,
                        s.LastName,
                        s.StudentCode,
                        AttendanceStatus.Present,
                        null))
                .ToListAsync(ct);

            return new ClassAttendanceDto(
                Guid.Empty,
                date,
                false,
                students);
        }

        if (session.IsLocked)
        {
            var records = await _db.AttendanceRecords.AsNoTracking()
                .Where(r => r.AttendanceSessionId == session.Id)
                .Join(_db.StudentEnrollments,
                    r => r.StudentEnrollmentId, e => e.Id,
                    (r, e) => new { r, e })
                .Join(_db.Students,
                    x => x.e.StudentId, s => s.Id,
                    (x, s) => new StudentAttendanceDto(
                        x.e.Id,
                        s.FirstName,
                        s.LastName,
                        s.StudentCode,
                        x.r.Status,
                        x.r.Note))
                .ToListAsync(ct);

            return new ClassAttendanceDto(
                session.Id,
                session.Date,
                session.IsLocked,
                records);
        }

        var freshStudents = await enrollmentQuery
            .Join(_db.Students,
                e => e.StudentId, s => s.Id,
                (e, s) => new StudentAttendanceDto(
                    e.Id,
                    s.FirstName,
                    s.LastName,
                    s.StudentCode,
                    AttendanceStatus.Present,
                    null))
            .ToListAsync(ct);

        return new ClassAttendanceDto(
            session.Id,
            session.Date,
            session.IsLocked,
            freshStudents);
    }

    public async Task<PagedResult<StudentAttendanceSummaryDto>> GetStudentAttendanceAsync(
        Guid enrollmentId, Guid? academicYearId, DateOnly? dateFrom, DateOnly? dateTo,
        AttendanceStatus? status, PaginationParams p, CancellationToken ct = default)
    {
        var query = _db.AttendanceRecords.AsNoTracking()
            .Where(ar => ar.StudentEnrollmentId == enrollmentId)
            .Join(_db.AttendanceSessions,
                ar => ar.AttendanceSessionId, asess => asess.Id,
                (ar, asess) => new { ar, Session = asess })
            .Join(_db.ClassSchedules,
                x => x.Session.ClassScheduleId, cs => cs.Id,
                (x, cs) => new { x.ar, x.Session, cs })
            .Join(_db.TeachingAssignments,
                x => x.cs.TeachingAssignmentId, ta => ta.Id,
                (x, ta) => new { x.ar, x.Session, x.cs, ta })
            .Join(_db.Subjects,
                x => x.ta.SubjectId, s => s.Id,
                (x, s) => new { x.ar, x.Session, x.cs, x.ta, Subject = s })
            .AsQueryable();

        if (academicYearId.HasValue)
        {
            var termIds = await _db.Terms.AsNoTracking()
                .Where(t => t.AcademicYearId == academicYearId.Value)
                .Select(t => t.Id)
                .ToListAsync(ct);
            query = query.Where(x => termIds.Contains(x.ta.TermId));
        }

        if (dateFrom.HasValue)
            query = query.Where(x => x.Session.Date >= dateFrom.Value);

        if (dateTo.HasValue)
            query = query.Where(x => x.Session.Date <= dateTo.Value);

        if (status.HasValue)
            query = query.Where(x => x.ar.Status == status.Value);

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(x => x.Session.Date)
            .Skip(p.Skip)
            .Take(p.PageSize)
            .Select(x => new StudentAttendanceSummaryDto(
                x.Session.Date,
                x.Subject.Name,
                x.Subject.Code,
                x.ar.Status,
                x.ar.Note))
            .ToListAsync(ct);

        return new PagedResult<StudentAttendanceSummaryDto>(items, total, p.Page, p.PageSize);
    }

    public async Task<StudentAttendanceSummaryResponse?> GetStudentAttendanceSummaryAsync(
        Guid enrollmentId, CancellationToken ct = default)
    {
        var enrollment = await _db.StudentEnrollments.AsNoTracking()
            .Where(e => e.Id == enrollmentId)
            .Join(_db.Students, e => e.StudentId, s => s.Id, (e, s) => new { e, Student = s })
            .FirstOrDefaultAsync(ct);

        if (enrollment is null) return null;

        var records = await _db.AttendanceRecords.AsNoTracking()
            .Where(ar => ar.StudentEnrollmentId == enrollmentId)
            .ToListAsync(ct);

        var totalDays = records.Count;
        var presentDays = records.Count(r => r.Status == AttendanceStatus.Present);
        var absentDays = records.Count(r => r.Status == AttendanceStatus.Absent);
        var lateDays = records.Count(r => r.Status == AttendanceStatus.Late);
        var pct = totalDays > 0 ? Math.Round(presentDays * 100m / totalDays, 1) : 100m;

        var detailedRecords = await _db.AttendanceRecords.AsNoTracking()
            .Where(ar => ar.StudentEnrollmentId == enrollmentId)
            .Join(_db.AttendanceSessions,
                ar => ar.AttendanceSessionId, asess => asess.Id,
                (ar, asess) => new { ar, Session = asess })
            .Join(_db.ClassSchedules,
                x => x.Session.ClassScheduleId, cs => cs.Id,
                (x, cs) => new { x.ar, x.Session, cs })
            .Join(_db.TeachingAssignments,
                x => x.cs.TeachingAssignmentId, ta => ta.Id,
                (x, ta) => new { x.ar, x.Session, x.cs, ta })
            .Join(_db.Subjects,
                x => x.ta.SubjectId, s => s.Id,
                (x, s) => new { x.ar, x.Session, x.cs, x.ta, Subject = s })
            .OrderByDescending(x => x.Session.Date)
            .Select(x => new StudentAttendanceSummaryDto(
                x.Session.Date,
                x.Subject.Name,
                x.Subject.Code,
                x.ar.Status,
                x.ar.Note))
            .ToListAsync(ct);

        return new StudentAttendanceSummaryResponse(
            enrollmentId,
            $"{enrollment.Student.FirstName} {enrollment.Student.LastName}",
            totalDays,
            presentDays,
            absentDays,
            lateDays,
            pct,
            detailedRecords);
    }
}
