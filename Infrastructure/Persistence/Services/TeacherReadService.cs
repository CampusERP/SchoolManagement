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

    public TeacherReadService(ApplicationDbContext db)
    {
        _db = db;
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
            .Select(t => new TeacherListDto(
                t.Id, t.EmployeeCode, t.FirstName, t.LastName,
                t.EmploymentStatus.ToString(),
                _db.TeachingAssignments.Count(a => a.TeacherId == t.Id)))
            .ToListAsync(ct);

        return new PagedResult<TeacherListDto>(items, total, pagination.Page, pagination.PageSize);
    }

    public async Task<TeacherDetailDto?> GetTeacherByIdAsync(Guid schoolId, Guid teacherId, CancellationToken ct)
    {
        var teacher = await _db.Teachers.AsNoTracking()
            .Where(t => t.SchoolId == schoolId && t.Id == teacherId)
            .Select(t => new TeacherDetailDto(
                t.Id, t.EmployeeCode, t.FirstName, t.LastName,
                t.EmploymentStatus.ToString(),
                _db.TeachingAssignments.Where(a => a.TeacherId == t.Id)
                    .Join(_db.ClassRooms, a => a.ClassRoomId, c => c.Id, (a, c) => new { a, c })
                    .Join(_db.Terms, ac => ac.a.TermId, trm => trm.Id, (ac, trm) => new TeachingAssignmentSummaryDto(
                        ac.a.Id, ac.a.SubjectId.ToString(), ac.c.Name, trm.Name)).ToList()))
            .FirstOrDefaultAsync(ct);

        return teacher;
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
            .Join(_db.Terms, ac => ac.a.TermId, trm => trm.Id, (ac, trm) => new TeachingAssignmentSummaryDto(
                ac.a.Id, ac.a.SubjectId.ToString(), ac.c.Name, trm.Name))
            .ToListAsync(ct);

        return items;
    }
}
