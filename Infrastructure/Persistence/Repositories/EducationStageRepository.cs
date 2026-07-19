using Microsoft.EntityFrameworkCore;
using Application.Common.Interfaces.Repositories;
using Domain.Entities.Academics;

namespace Infrastructure.Persistence.Repositories;

public class EducationStageRepository : IEducationStageRepository
{
    private readonly ApplicationDbContext _context;

    public EducationStageRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<EducationStage?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await _context.EducationStages.FirstOrDefaultAsync(s => s.Id == id, ct);

    public async Task<List<EducationStage>> GetAllAsync(CancellationToken ct = default) =>
        await _context.EducationStages.AsNoTracking().OrderBy(s => s.Name).ToListAsync(ct);

    public async Task AddAsync(EducationStage educationStage, CancellationToken ct = default) =>
        await _context.EducationStages.AddAsync(educationStage, ct);

    public async Task<bool> ExistsByNameAsync(string name, CancellationToken ct = default) =>
        await _context.EducationStages.AnyAsync(s => s.Name == name, ct);

    public Task RemoveAsync(EducationStage educationStage, CancellationToken ct = default)
    {
        _context.EducationStages.Remove(educationStage);
        return Task.CompletedTask;
    }
}
