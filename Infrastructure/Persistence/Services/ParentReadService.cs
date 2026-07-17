using Application.Common.Interfaces.Services;
using Application.Common.Models;
using Application.Features.People.Queries.GetMyChildren;
using Application.Features.People.Queries.GetParents;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Services;

public class ParentReadService : IParentReadService
{
    private readonly ApplicationDbContext _db;

    public ParentReadService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<PagedResult<ParentListDto>> GetParentsAsync(Guid schoolId, string? searchTerm, PaginationParams pagination, CancellationToken ct)
    {
        var query = _db.Parents.AsNoTracking().Where(p => p.SchoolId == schoolId);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.ToLower();
            query = query.Where(p =>
                p.FirstName.ToLower().Contains(term) ||
                p.LastName.ToLower().Contains(term));
        }

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderBy(p => p.LastName)
            .Skip(pagination.Skip).Take(pagination.PageSize)
            .Select(p => new ParentListDto(
                p.Id, p.FirstName, p.LastName,
                _db.StudentGuardians.Count(g => g.ParentId == p.Id)))
            .ToListAsync(ct);

        return new PagedResult<ParentListDto>(items, total, pagination.Page, pagination.PageSize);
    }

    public async Task<ParentDetailDto?> GetParentByIdAsync(Guid schoolId, Guid parentId, CancellationToken ct)
    {
        var parent = await _db.Parents.AsNoTracking()
            .Where(p => p.SchoolId == schoolId && p.Id == parentId)
            .Select(p => new ParentDetailDto(
                p.Id,
                p.FirstName,
                p.LastName,
                _db.StudentGuardians
                    .Where(g => g.ParentId == p.Id)
                    .Join(_db.Students, g => g.StudentId, s => s.Id, (g, s) => new { g, s })
                    .Select(x => new ChildSummaryDto(
                        x.s.Id,
                        _db.StudentEnrollments
                            .Where(e => e.StudentId == x.s.Id && e.Status == Domain.Enums.EnrollmentStatus.Active)
                            .Select(e => e.Id)
                            .FirstOrDefault(),
                        x.s.StudentCode,
                        x.s.FirstName,
                        x.s.LastName,
                        x.g.RelationshipType.ToString(),
                        x.g.IsPrimaryContact,
                        x.g.CanViewGrades,
                        x.g.CanViewBilling,
                        _db.StudentEnrollments
                            .Where(e => e.StudentId == x.s.Id && e.Status == Domain.Enums.EnrollmentStatus.Active)
                            .Join(_db.ClassRooms, e => e.ClassRoomId, c => c.Id, (e, c) => c.Name)
                            .FirstOrDefault()))
                    .ToList()))
            .FirstOrDefaultAsync(ct);

        return parent;
    }

    public async Task<List<ChildSummaryDto>> GetMyChildrenAsync(Guid schoolId, Guid userId, CancellationToken ct)
    {
        var parentId = await _db.Parents.AsNoTracking()
            .Where(p => p.SchoolId == schoolId && p.ApplicationUserId == userId)
            .Select(p => p.Id)
            .FirstOrDefaultAsync(ct);

        if (parentId == Guid.Empty) return new List<ChildSummaryDto>();

        var children = await _db.StudentGuardians.AsNoTracking()
            .Where(g => g.ParentId == parentId)
            .Join(_db.Students, g => g.StudentId, s => s.Id, (g, s) => new { Guardian = g, Student = s })
            .Select(x => new {
                Guardian = x.Guardian,
                Student = x.Student,
                Enrollment = _db.StudentEnrollments
                    .Where(e => e.StudentId == x.Student.Id && e.Status == Domain.Enums.EnrollmentStatus.Active)
                    .Join(_db.ClassRooms, e => e.ClassRoomId, c => c.Id, (e, c) => new { e.Id, ClassName = c.Name })
                    .FirstOrDefault()
            })
            .ToListAsync(ct);

        return children.Select(x => new ChildSummaryDto(
            x.Student.Id,
            x.Enrollment?.Id ?? Guid.Empty,
            x.Student.StudentCode,
            x.Student.FirstName,
            x.Student.LastName,
            x.Guardian.RelationshipType.ToString(),
            x.Guardian.IsPrimaryContact,
            x.Guardian.CanViewGrades,
            x.Guardian.CanViewBilling,
            x.Enrollment?.ClassName
        )).ToList();
    }
}
