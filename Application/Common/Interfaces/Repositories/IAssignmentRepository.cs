using Domain.Entities.Assignments;

namespace Application.Common.Interfaces.Repositories;

public interface IAssignmentRepository
{
    Task<Assignment?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(Assignment assignment, CancellationToken ct = default);
}
