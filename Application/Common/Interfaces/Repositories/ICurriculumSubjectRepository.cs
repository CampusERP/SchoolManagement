using Domain.Entities.Academics;

namespace Application.Common.Interfaces.Repositories;

public interface ICurriculumSubjectRepository
{
    Task<bool> ExistsAsync(Guid gradeLevelId, Guid subjectId, CancellationToken ct = default);
    void Add(CurriculumSubject curriculumSubject);
}
