using Domain.Entities.Academics;

namespace Application.Common.Interfaces.Repositories;

public interface ISubjectRepository
{
    Task<bool> ExistsAsync(string code, CancellationToken ct);
    Task<bool> ExistsForSchoolAsync(string code, Guid schoolId, Guid? exceptSubjectId = null, CancellationToken ct = default);
    Task<bool> HasAcademicUsageAsync(Guid subjectId, Guid schoolId, CancellationToken ct = default);
    Task<Subject?> GetByIdAsync(Guid id, CancellationToken ct = default);
    void Add(Subject subject);
    void Remove(Subject subject);
}
