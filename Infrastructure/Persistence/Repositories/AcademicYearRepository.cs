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

    public Task DeleteAsync(AcademicYear year, CancellationToken ct = default)
    {
        _context.AcademicYears.Remove(year);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Marks a newly added child Term for insertion. The mediator transaction
    /// behavior owns the save boundary for commands.
    /// </summary>
    public Task<Guid> AddTermAsync(AcademicYear year, CancellationToken ct = default)
    {
        var term = year.Terms.Last();
        _context.Entry(year).State = EntityState.Unchanged;
        _context.Entry(term).State = EntityState.Added;
        return Task.FromResult(term.Id);
    }

    public Task UpdateTermAsync(AcademicYear year, Term term, CancellationToken ct = default)
    {
        _context.Entry(year).State = EntityState.Unchanged;
        _context.Entry(term).State = EntityState.Modified;
        return Task.CompletedTask;
    }

    public Task DeleteTermAsync(AcademicYear year, Term term, CancellationToken ct = default)
    {
        _context.Entry(year).State = EntityState.Unchanged;
        _context.Entry(term).State = EntityState.Deleted;
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        await _context.SaveChangesAsync(ct);
    }
}
