using Domain.Entities.People;

namespace Application.Common.Interfaces.Repositories;

public interface IParentRepository
{
    Task<Parent?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(Parent parent, CancellationToken ct = default);
}
