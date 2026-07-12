using Domain.Entities.Academics;

namespace Application.Common.Interfaces.Repositories;

public interface IGradeLevelRepository
{
    Task<GradeLevel?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(GradeLevel gradeLevel, CancellationToken ct = default);
}
