using Domain.Entities.Academics;

namespace Application.Common.Interfaces.Repositories;

public interface ISubjectBookRepository
{
    Task<SubjectBook?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<SubjectBook?> GetByIdAndCurriculumAsync(Guid id, Guid curriculumSubjectId, CancellationToken ct = default);
    Task<IReadOnlyList<SubjectBook>> GetByCurriculumSubjectAsync(Guid curriculumSubjectId, CancellationToken ct = default);
    Task AddAsync(SubjectBook book, CancellationToken ct = default);
    void Remove(SubjectBook book);
}
