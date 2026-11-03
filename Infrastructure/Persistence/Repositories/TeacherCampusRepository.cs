using Application.Common.Interfaces.Repositories;
using Domain.Entities.People;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public sealed class TeacherCampusRepository : ITeacherCampusRepository
{
    private readonly ApplicationDbContext _db;
    public TeacherCampusRepository(ApplicationDbContext db) => _db = db;

    public Task<List<Guid>> GetCampusIdsAsync(Guid teacherId, CancellationToken ct = default) =>
        _db.TeacherCampuses.AsNoTracking().Where(x => x.TeacherId == teacherId).Select(x => x.CampusId).ToListAsync(ct);

    public async Task SetCampusIdsAsync(Guid schoolId, Guid teacherId, IReadOnlyCollection<Guid> campusIds, CancellationToken ct = default)
    {
        var existing = await _db.TeacherCampuses.Where(x => x.TeacherId == teacherId).ToListAsync(ct);
        _db.TeacherCampuses.RemoveRange(existing);
        foreach (var campusId in campusIds.Distinct())
            await _db.TeacherCampuses.AddAsync(TeacherCampus.Create(schoolId, teacherId, campusId), ct);
    }
}
