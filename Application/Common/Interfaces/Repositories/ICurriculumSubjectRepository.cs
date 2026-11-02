using Domain.Entities.Academics;

namespace Application.Common.Interfaces.Repositories;

public interface ICurriculumSubjectRepository
{
    Task<bool> ExistsAsync(Guid gradeLevelId, Guid subjectId, CancellationToken ct = default);
    Task<CurriculumSubject?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<CurriculumSubject>> GetByGradeLevelAsync(Guid gradeLevelId, CancellationToken ct = default);
    Task<IReadOnlyList<CurriculumSubject>> GetBySubjectAsync(Guid subjectId, CancellationToken ct = default);
    void Add(CurriculumSubject curriculumSubject);
    void Remove(CurriculumSubject curriculumSubject);
}
