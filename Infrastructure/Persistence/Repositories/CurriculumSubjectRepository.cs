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

    public void Add(CurriculumSubject curriculumSubject) =>
        _db.CurriculumSubjects.Add(curriculumSubject);
}
