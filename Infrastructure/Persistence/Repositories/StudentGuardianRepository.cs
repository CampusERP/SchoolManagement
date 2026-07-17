using Application.Common.Interfaces.Repositories;
using Domain.Entities.People;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class StudentGuardianRepository : IStudentGuardianRepository
{
    private readonly ApplicationDbContext _context;

    public StudentGuardianRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(StudentGuardian guardian, CancellationToken ct = default) =>
        await _context.StudentGuardians.AddAsync(guardian, ct);

    public Task<bool> ExistsAsync(Guid studentId, Guid parentId, CancellationToken ct = default)
    {
        return _context.StudentGuardians.AnyAsync(x => x.StudentId == studentId && x.ParentId == parentId, ct);
    }
}