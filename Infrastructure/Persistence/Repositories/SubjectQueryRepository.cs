using Application.Common.Interfaces.Repositories;
using Application.Features.Academics.Queries.GetSubjects;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class SubjectQueryRepository : ISubjectQueryRepository
{
    private readonly ApplicationDbContext _db;
    public SubjectQueryRepository(ApplicationDbContext db) => _db = db;

    public async Task<List<SubjectDto>> GetSubjectsAsync(Guid? gradeLevelId, CancellationToken cancellationToken = default)
    {
        var query = _db.Subjects.AsNoTracking();
        if (gradeLevelId.HasValue)
        {
            var subjectIds = _db.CurriculumSubjects
                .Where(cs => cs.GradeLevelId == gradeLevelId.Value)
                .Select(cs => cs.SubjectId);
            query = query.Where(s => subjectIds.Contains(s.Id));
        }
        return await query.Select(s => new SubjectDto(s.Id, s.Code, s.Name, s.Description))
            .ToListAsync(cancellationToken);
    }
}
