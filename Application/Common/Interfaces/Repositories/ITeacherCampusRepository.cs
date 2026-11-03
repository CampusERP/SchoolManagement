using Domain.Entities.People;

namespace Application.Common.Interfaces.Repositories;

public interface ITeacherCampusRepository
{
    Task<List<Guid>> GetCampusIdsAsync(Guid teacherId, CancellationToken ct = default);
    Task SetCampusIdsAsync(Guid schoolId, Guid teacherId, IReadOnlyCollection<Guid> campusIds, CancellationToken ct = default);
}
