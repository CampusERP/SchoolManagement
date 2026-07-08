using Microsoft.EntityFrameworkCore;
using Application.Common.Interfaces.Repositories;
using Domain.Entities.Academics;

namespace Infrastructure.Persistence.Repositories;

public class GradeLevelRepository : IGradeLevelRepository
{
    private readonly ApplicationDbContext _context;

    public GradeLevelRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<GradeLevel?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await _context.GradeLevels.FirstOrDefaultAsync(g => g.Id == id, ct);

    public async Task AddAsync(GradeLevel gradeLevel, CancellationToken ct = default) =>
        await _context.GradeLevels.AddAsync(gradeLevel, ct);
}
