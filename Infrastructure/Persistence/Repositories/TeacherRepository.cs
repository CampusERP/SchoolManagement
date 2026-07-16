using Microsoft.EntityFrameworkCore;
using Application.Common.Interfaces.Repositories;
using Application.Common.Models;
using Application.Features.People.Queries.GetTeachers;
using Domain.Entities.People;

namespace Infrastructure.Persistence.Repositories;

public class TeacherRepository : ITeacherRepository
{
    private readonly ApplicationDbContext _context;

    public TeacherRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Teacher?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await _context.Teachers.FirstOrDefaultAsync(t => t.Id == id, ct);

    public async Task<bool> ExistsAsync(Guid schoolId, string employeeCode, CancellationToken ct = default) =>
        await _context.Teachers.AnyAsync(t => t.SchoolId == schoolId && t.EmployeeCode == employeeCode, ct);

    public async Task AddAsync(Teacher teacher, CancellationToken ct = default) =>
        await _context.Teachers.AddAsync(teacher, ct);

    public async Task<PagedResult<GetTeachersDto>> GetTeachersAsync(
        Guid schoolId,
        PaginationParams pagination,
        CancellationToken ct = default)
    {
        var teachersQuery = _context.Teachers
            .AsNoTracking()
            .Where(t => t.SchoolId == schoolId);

        var totalCount = await teachersQuery.CountAsync(ct);

        var items = await teachersQuery
            .OrderBy(t => t.FirstName)
            .Skip(pagination.Skip)
            .Take(pagination.PageSize)
            .Select(t => new GetTeachersDto(
                t.Id,
                t.EmployeeCode,
                t.FirstName,
                t.LastName,
                t.EmploymentStatus.ToString()
            ))
            .ToListAsync(ct);

        return new PagedResult<GetTeachersDto>(items, totalCount, pagination.Page, pagination.PageSize);
    }
}
