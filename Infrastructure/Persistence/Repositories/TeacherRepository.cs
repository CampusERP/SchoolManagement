using Microsoft.EntityFrameworkCore;
using Application.Common.Interfaces.Repositories;
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
}
