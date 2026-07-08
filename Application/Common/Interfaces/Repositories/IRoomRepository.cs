using Domain.Entities.Academics;

namespace Application.Common.Interfaces.Repositories;

public interface IRoomRepository
{
    Task<Room?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<bool> ExistsAsync(Guid schoolId, string name, CancellationToken ct = default);
    Task AddAsync(Room room, CancellationToken ct = default);
}
