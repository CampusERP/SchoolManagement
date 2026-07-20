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

    /// <summary>
    /// Persists a newly added child Term. EF mis-classifies the new Term as
    /// Modified (its RowVersion concurrency token is value-generated), which
    /// produces an UPDATE instead of an INSERT and fails with a
    /// DbUpdateConcurrencyException. We force the Term to Added and the
    /// unchanged aggregate root to Unchanged so only the Term is inserted.
    /// </summary>
    public async Task<Guid> AddTermAsync(AcademicYear year, CancellationToken ct = default)
    {
        var term = year.Terms.Last();
        _context.Entry(year).State = EntityState.Unchanged;
        _context.Entry(term).State = EntityState.Added;
        await _context.SaveChangesAsync(ct);
        return term.Id;
    }

    public async Task UpdateTermAsync(AcademicYear year, Term term, CancellationToken ct = default)
    {
        _context.Entry(year).State = EntityState.Unchanged;
        _context.Entry(term).State = EntityState.Modified;
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteTermAsync(AcademicYear year, Term term, CancellationToken ct = default)
    {
        _context.Entry(year).State = EntityState.Unchanged;
        _context.Entry(term).State = EntityState.Deleted;
        await _context.SaveChangesAsync(ct);
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        await _context.SaveChangesAsync(ct);
    }
}
