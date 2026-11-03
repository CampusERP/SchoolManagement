using Application.Common.Interfaces.Repositories;
using Domain.Entities.Tenancy;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class SchoolRepository : ISchoolRepository
{
    private readonly PlatformDbContext _context;
    private readonly ApplicationDbContext _applicationDb;

    public SchoolRepository(PlatformDbContext context, ApplicationDbContext applicationDb)
    {
        _context = context;
        _applicationDb = applicationDb;
    }

    public async Task AddAsync(School school, CancellationToken ct = default)
    {
        await _context.Schools.AddAsync(school, ct);
    }

    public async Task<School?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Schools.FindAsync(id, ct);
    }

    public async Task<School?> GetBySubdomainAsync(string subdomain, CancellationToken ct = default)
    {
        return await _context.Schools.FirstOrDefaultAsync(s => s.SubdomainCode == subdomain, ct);
    }

    public async Task<IReadOnlyList<Guid>> GetActiveSchoolIdsAsync(
        IReadOnlyCollection<Guid> schoolIds, CancellationToken ct = default)
    {
        if (schoolIds.Count == 0)
            return [];

        return await _context.Schools.AsNoTracking()
            .Where(s => schoolIds.Contains(s.Id) && !s.IsDeleted && s.Status == "Active")
            .Select(s => s.Id)
            .ToListAsync(ct);
    }

    /// <summary>
    /// Persists a newly added child Campus. EF mis-classifies the new Campus as
    /// Modified (its RowVersion concurrency token is value-generated), which
    /// produces an UPDATE instead of an INSERT and fails with a
    /// DbUpdateConcurrencyException. We force the Campus to Added and the
    /// unchanged aggregate root to Unchanged so only the Campus is inserted.
    /// </summary>
    public async Task<Guid> AddCampusAsync(School school, CancellationToken ct = default)
    {
        var campus = school.Campuses.Last();
        _context.Entry(school).State = EntityState.Unchanged;
        _context.Entry(campus).State = EntityState.Added;
        return campus.Id;
    }

    public Task<Campus?> GetCampusAsync(Guid schoolId, Guid campusId, CancellationToken ct = default) =>
        _context.Campuses.FirstOrDefaultAsync(c => c.SchoolId == schoolId && c.Id == campusId, ct);

    public async Task<IReadOnlyList<Campus>> GetCampusesAsync(Guid schoolId, CancellationToken ct = default) =>
        await _context.Campuses.AsNoTracking().Where(c => c.SchoolId == schoolId).ToListAsync(ct);

    public async Task<bool> IsCampusInUseAsync(Guid schoolId, Guid campusId, CancellationToken ct = default) =>
        await _applicationDb.Students.AnyAsync(x => x.SchoolId == schoolId && x.CampusId == campusId, ct) ||
        await _applicationDb.Rooms.AnyAsync(x => x.SchoolId == schoolId && x.CampusId == campusId, ct) ||
        await _applicationDb.ClassRooms.AnyAsync(x => x.SchoolId == schoolId && x.CampusId == campusId, ct) ||
        await _applicationDb.TeacherCampuses.AnyAsync(x => x.SchoolId == schoolId && x.CampusId == campusId, ct);

    public async Task AddCampusEntityAsync(Campus campus, CancellationToken ct = default) =>
        await _context.Campuses.AddAsync(campus, ct);

    public Task DeleteCampusAsync(Campus campus, CancellationToken ct = default)
    {
        _context.Campuses.Remove(campus);
        return Task.CompletedTask;
    }
}
