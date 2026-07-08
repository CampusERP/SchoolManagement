using Application.Common.Interfaces.Repositories;
using Domain.Entities.People;

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
}
