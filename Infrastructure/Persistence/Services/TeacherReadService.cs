using Application.Common.Interfaces.Services;
using Application.Common.Models;
using Application.Features.People.Queries.GetMyClasses;
using Application.Features.People.Queries.GetTeacherById;
using Application.Features.People.Queries.GetTeachers;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Services;

public class TeacherReadService : ITeacherReadService
{
    private readonly ApplicationDbContext _db;
    private readonly PlatformDbContext _platformDb;

    public TeacherReadService(ApplicationDbContext db, PlatformDbContext platformDb)
    {
        _db = db;
        _platformDb = platformDb;
    }

    public async Task<PagedResult<TeacherListDto>> GetTeachersAsync(Guid schoolId, string? searchTerm, PaginationParams pagination, CancellationToken ct)
    {
        var query = _db.Teachers.AsNoTracking().Where(t => t.SchoolId == schoolId);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.ToLower();
            query = query.Where(t =>
                t.FirstName.ToLower().Contains(term) ||
                t.LastName.ToLower().Contains(term) ||
                t.EmployeeCode.ToLower().Contains(term));
        }

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderBy(t => t.LastName)
            .Skip(pagination.Skip).Take(pagination.PageSize)
            .Select(t => new {
                t.Id, t.EmployeeCode, t.FirstName, t.LastName,
                t.ApplicationUserId,
                EmploymentStatus = t.EmploymentStatus.ToString(),
                AssignedClassesCount = _db.TeachingAssignments.Count(a => a.TeacherId == t.Id)
            })
            .ToListAsync(ct);

        var userIds = items.Where(x => x.ApplicationUserId != Guid.Empty).Select(x => x.ApplicationUserId).ToList();
        var emails = userIds.Count > 0
            ? await _platformDb.Users
                .Where(u => userIds.Contains(u.Id))
                .Select(u => new { u.Id, u.Email })
                .ToDictionaryAsync(u => u.Id, u => u.Email, ct)
            : new Dictionary<Guid, string?>();

        var result = items.Select(x => new TeacherListDto(
            x.Id, x.EmployeeCode, x.FirstName, x.LastName,
            emails.GetValueOrDefault(x.ApplicationUserId),
            x.EmploymentStatus,
            x.AssignedClassesCount)).ToList();

        return new PagedResult<TeacherListDto>(result, total, pagination.Page, pagination.PageSize);
    }

    public async Task<TeacherDetailDto?> GetTeacherByIdAsync(Guid schoolId, Guid teacherId, CancellationToken ct)
    {
        var t = await _db.Teachers.AsNoTracking()
            .Where(t => t.SchoolId == schoolId && t.Id == teacherId)
            .Select(t => new { t.Id, t.EmployeeCode, t.FirstName, t.LastName, t.ApplicationUserId, EmploymentStatus = t.EmploymentStatus.ToString() })
            .FirstOrDefaultAsync(ct);

        if (t is null) return null;

        string? email = null;
        if (t.ApplicationUserId != Guid.Empty)
        {
            email = await _platformDb.Users
                .Where(u => u.Id == t.ApplicationUserId)
                .Select(u => u.Email)
                .FirstOrDefaultAsync(ct);
        }

        var teacher = new TeacherDetailDto(
            t.Id, t.EmployeeCode, t.FirstName, t.LastName, email,
            t.EmploymentStatus, new List<TeachingAssignmentDetailDto>());

        var assignments = await _db.TeachingAssignments.AsNoTracking()
            .Where(a => a.TeacherId == teacherId)
            .Join(_db.ClassRooms, a => a.ClassRoomId, c => c.Id, (a, c) => new { a, c })
            .Join(_db.Terms, ac => ac.a.TermId, trm => trm.Id, (ac, trm) => new { ac, trm })
            .Join(_db.Subjects, x => x.ac.a.SubjectId, s => s.Id, (x, s) => new { x.ac.a, x.ac.c, x.trm, s })
            .Select(x => new
            {
                x.a.Id,
                x.a.SubjectId,
                x.a.ClassRoomId,
                x.a.TermId,
                SubjectName = x.s.Name,
                ClassRoomName = x.c.Name,
                TermName = x.trm.Name
            })
            .ToListAsync(ct);

        var assignmentIds = assignments.Select(a => a.Id).ToList();

        var schedules = await _db.ClassSchedules.AsNoTracking()
            .Where(cs => assignmentIds.Contains(cs.TeachingAssignmentId))
            .Join(_db.Rooms, cs => cs.RoomId, r => r.Id, (cs, r) => new { cs, r })
            .ToListAsync(ct);

        var scheduleGroups = schedules
            .GroupBy(s => s.cs.TeachingAssignmentId)
            .ToDictionary(g => g.Key, g => g.Select(s => new TeachingAssignmentScheduleSlotDto(
                s.cs.Id,
                (int)s.cs.DayOfWeek,
                s.cs.StartTime.ToString(@"hh\:mm"),
                s.cs.EndTime.ToString(@"hh\:mm"),
                s.r.Id,
                s.r.Name
            )).ToList());

        var result = assignments.Select(a => new TeachingAssignmentDetailDto(
            a.Id,
            a.SubjectId,
            a.SubjectName,
            a.ClassRoomId,
            a.ClassRoomName,
            a.TermId,
            a.TermName,
            scheduleGroups.GetValueOrDefault(a.Id, new List<TeachingAssignmentScheduleSlotDto>())
        )).ToList();

        return teacher with { Assignments = result };
    }

    public async Task<List<TeachingAssignmentSummaryDto>> GetMyClassesAsync(Guid schoolId, Guid userId, Guid termId, CancellationToken ct)
    {
        var teacherId = await _db.Teachers.AsNoTracking()
            .Where(t => t.SchoolId == schoolId && t.ApplicationUserId == userId)
            .Select(t => t.Id)
            .FirstOrDefaultAsync(ct);

        if (teacherId == Guid.Empty) return new List<TeachingAssignmentSummaryDto>();

        var items = await _db.TeachingAssignments.AsNoTracking()
            .Where(a => a.TeacherId == teacherId && a.TermId == termId)
            .Join(_db.ClassRooms, a => a.ClassRoomId, c => c.Id, (a, c) => new { a, c })
            .Join(_db.Terms, ac => ac.a.TermId, trm => trm.Id, (ac, trm) => new { ac, trm })
            .Join(_db.Subjects, x => x.ac.a.SubjectId, s => s.Id, (x, s) => new TeachingAssignmentSummaryDto(
                x.ac.a.Id, s.Name, x.ac.c.Name, x.trm.Name))
            .ToListAsync(ct);

        return items;
    }
}
