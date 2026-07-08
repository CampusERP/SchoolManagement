using Microsoft.EntityFrameworkCore;
using Application.Common.Interfaces.Repositories;
using Domain.Entities.People;

namespace Infrastructure.Persistence.Repositories;

public class StudentRepository : IStudentRepository
{
    private readonly ApplicationDbContext _context;

    public StudentRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Student?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await _context.Students.FirstOrDefaultAsync(s => s.Id == id, ct);

    public async Task<bool> ExistsAsync(Guid schoolId, string studentCode, CancellationToken ct = default) =>
        await _context.Students.AnyAsync(s => s.SchoolId == schoolId && s.StudentCode == studentCode, ct);

    public async Task AddAsync(Student student, CancellationToken ct = default) =>
        await _context.Students.AddAsync(student, ct);
}
