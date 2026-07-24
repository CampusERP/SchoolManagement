using Application.Common.Interfaces.Services;
using Application.Common.Models;
using Application.Features.Schools.Queries.GetAllSchools;
using Application.Features.Schools.Queries.GetPlatformAnalytics;
using Application.Features.Schools.Queries.GetSchoolById;
using Application.Features.Schools.Queries.GetSchoolDashboard;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Services;

public class SchoolReadService : ISchoolReadService
{
    private readonly PlatformDbContext _platformDb;
    private readonly ApplicationDbContext _appDb;

    public SchoolReadService(PlatformDbContext platformDb, ApplicationDbContext appDb)
    {
        _platformDb = platformDb;
        _appDb = appDb;
    }

    public async Task<PagedResult<SchoolListDto>> GetSchoolsAsync(PaginationParams pagination, CancellationToken ct = default)
    {
        var query = _platformDb.Schools.AsNoTracking().Where(s => !s.IsDeleted);
        var total = await query.CountAsync(ct);
        var items = await query
            .OrderBy(s => s.Name)
            .Skip(pagination.Skip).Take(pagination.PageSize)
            .Select(s => new SchoolListDto(s.Id, s.Name, s.SubdomainCode, s.Status, 0, 0, s.CreatedAtUtc))
            .ToListAsync(ct);
        
        return new PagedResult<SchoolListDto>(items, total, pagination.Page, pagination.PageSize);
    }

    public async Task<SchoolDetailDto?> GetSchoolByIdAsync(Guid schoolId, CancellationToken ct = default)
    {
        return await _platformDb.Schools.AsNoTracking()
            .Where(s => s.Id == schoolId && !s.IsDeleted)
            .Select(s => new SchoolDetailDto(
                s.Id, s.Name, s.SubdomainCode, s.Status,
                s.Campuses.Select(c => new CampusDto(c.Id, c.Name, c.Address)).ToList()))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<SchoolDashboardDto?> GetSchoolDashboardAsync(Guid schoolId, CancellationToken ct = default)
    {
        var school = await _platformDb.Schools.AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == schoolId, ct);
        if (school is null) return null;

        var currentYear = await _appDb.AcademicYears.AsNoTracking()
            .Where(y => y.IsCurrent)
            .Select(y => y.Name)
            .FirstOrDefaultAsync(ct);

        var recentStudents = await _appDb.StudentEnrollments.AsNoTracking()
            .OrderByDescending(e => e.EnrolledAtUtc)
            .Take(10)
            .Join(_appDb.Students.AsNoTracking(), enrollment => enrollment.StudentId, student => student.Id,
                (enrollment, student) => new { enrollment, student })
            .GroupJoin(_appDb.ClassRooms.AsNoTracking(), x => x.enrollment.ClassRoomId, classroom => classroom.Id,
                (x, classrooms) => new { x.enrollment, x.student, classroom = classrooms.FirstOrDefault() })
            .Select(x => new RecentStudentDto(
                x.student.Id,
                x.student.FirstName + " " + x.student.LastName,
                x.student.StudentCode,
                x.classroom != null ? x.classroom.Name : "Not assigned",
                x.enrollment.EnrolledAtUtc,
                x.enrollment.Status.ToString()))
            .ToListAsync(ct);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var attendance = await _appDb.AttendanceRecords.AsNoTracking()
            .Join(_appDb.AttendanceSessions.AsNoTracking().Where(s => s.Date == today),
                record => record.AttendanceSessionId, session => session.Id, (record, _) => record.Status)
            .GroupBy(status => status)
            .Select(group => new { Status = group.Key, Count = group.Count() })
            .ToListAsync(ct);

        var attendanceSummary = new AttendanceSummaryDto(
            attendance.Where(x => x.Status == Domain.Enums.AttendanceStatus.Present).Sum(x => x.Count),
            attendance.Where(x => x.Status == Domain.Enums.AttendanceStatus.Absent).Sum(x => x.Count),
            attendance.Where(x => x.Status == Domain.Enums.AttendanceStatus.Late).Sum(x => x.Count),
            attendance.Where(x => x.Status == Domain.Enums.AttendanceStatus.Excused).Sum(x => x.Count));

        return new SchoolDashboardDto(
            school.Name,
            await _appDb.Students.CountAsync(ct),
            await _appDb.Teachers.CountAsync(ct),
            await _appDb.Parents.CountAsync(ct),
            await _appDb.ClassRooms.CountAsync(ct),
            await _appDb.StudentEnrollments.CountAsync(e => e.Status == Domain.Enums.EnrollmentStatus.Active, ct),
            currentYear,
            recentStudents,
            attendanceSummary);
    }

    public async Task<PlatformAnalyticsDto> GetPlatformAnalyticsAsync(CancellationToken ct = default)
    {
        return new PlatformAnalyticsDto(
            TotalSchools: await _platformDb.Schools.CountAsync(s => !s.IsDeleted, ct),
            ActiveSchools: await _platformDb.Schools.CountAsync(s => !s.IsDeleted && s.Status == "Active", ct),
            SuspendedSchools: await _platformDb.Schools.CountAsync(s => !s.IsDeleted && s.Status != "Active", ct),
            TotalStudents: await _appDb.Students.IgnoreQueryFilters().CountAsync(s => !s.IsDeleted, ct),
            TotalParents: await _appDb.Parents.IgnoreQueryFilters().CountAsync(p => !p.IsDeleted, ct),
            TotalTeachers: await _appDb.Teachers.IgnoreQueryFilters().CountAsync(t => !t.IsDeleted, ct),
            TotalSchoolAdmins: await _appDb.SchoolAdminProfiles.IgnoreQueryFilters().CountAsync(a => !a.IsDeleted, ct),
            TotalUsers: await _platformDb.Users.CountAsync(ct));
    }
}
