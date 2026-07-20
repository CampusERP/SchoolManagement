using Domain.Entities.Academics;

namespace Application.Common.Interfaces.Repositories;

public interface IEducationStageRepository
{
    Task<EducationStage?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<EducationStage>> GetAllAsync(CancellationToken ct = default);
    Task AddAsync(EducationStage educationStage, CancellationToken ct = default);
    Task<bool> ExistsByNameAsync(string name, CancellationToken ct = default);
    Task RemoveAsync(EducationStage educationStage, CancellationToken ct = default);
}
