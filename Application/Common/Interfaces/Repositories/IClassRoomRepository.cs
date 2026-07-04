using Domain.Entities.Academics;

namespace Application.Common.Interfaces.Repositories;

public interface IClassRoomRepository
{
    Task<ClassRoom?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<bool> ExistsAsync(Guid schoolId, Guid gradeLevelId, Guid academicYearId, string name, CancellationToken ct = default);
    Task AddAsync(ClassRoom classRoom, CancellationToken ct = default);
}
