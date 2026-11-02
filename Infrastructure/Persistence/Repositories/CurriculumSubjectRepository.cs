using Application.Common.Interfaces.Repositories;
using Domain.Entities.Academics;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class CurriculumSubjectRepository : ICurriculumSubjectRepository
{
    private readonly ApplicationDbContext _db;
    public CurriculumSubjectRepository(ApplicationDbContext db) => _db = db;

    public async Task<bool> ExistsAsync(Guid gradeLevelId, Guid subjectId, CancellationToken ct = default) =>
        await _db.CurriculumSubjects.AnyAsync(
            cs => cs.GradeLevelId == gradeLevelId && cs.SubjectId == subjectId, ct);

    public Task<CurriculumSubject?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        _db.CurriculumSubjects.FirstOrDefaultAsync(cs => cs.Id == id, ct);

    public async Task<IReadOnlyList<CurriculumSubject>> GetByGradeLevelAsync(Guid gradeLevelId, CancellationToken ct = default) =>
        await _db.CurriculumSubjects.Where(cs => cs.GradeLevelId == gradeLevelId).ToListAsync(ct);

    public async Task<IReadOnlyList<CurriculumSubject>> GetBySubjectAsync(Guid subjectId, CancellationToken ct = default) =>
        await _db.CurriculumSubjects.Where(cs => cs.SubjectId == subjectId).ToListAsync(ct);

    public void Add(CurriculumSubject curriculumSubject) =>
        _db.CurriculumSubjects.Add(curriculumSubject);

    public void Remove(CurriculumSubject curriculumSubject) => _db.CurriculumSubjects.Remove(curriculumSubject);
}
