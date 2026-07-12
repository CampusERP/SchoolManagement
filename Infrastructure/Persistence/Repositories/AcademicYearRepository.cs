using Microsoft.EntityFrameworkCore;
using Application.Common.Interfaces.Repositories;
using Domain.Entities.Academics;

namespace Infrastructure.Persistence.Repositories;

public class AcademicYearRepository : IAcademicYearRepository
{
    private readonly ApplicationDbContext _context;

    public AcademicYearRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AcademicYear?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await _context.AcademicYears.Include(y => y.Terms).FirstOrDefaultAsync(y => y.Id == id, ct);

    public async Task<AcademicYear?> GetCurrentAsync(Guid schoolId, CancellationToken ct = default) =>
        await _context.AcademicYears.FirstOrDefaultAsync(y => y.SchoolId == schoolId && y.IsCurrent, ct);

    public async Task<bool> HasCurrentAsync(Guid schoolId, CancellationToken ct = default) =>
        await _context.AcademicYears.AnyAsync(y => y.SchoolId == schoolId && y.IsCurrent, ct);

    public async Task AddAsync(AcademicYear year, CancellationToken ct = default) =>
        await _context.AcademicYears.AddAsync(year, ct);
}
