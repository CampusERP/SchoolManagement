using Domain.Entities.Tenancy;

namespace Application.Common.Interfaces.Repositories;

public interface ISchoolRepository
{
    Task<School?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<School?> GetBySubdomainAsync(string subdomain, CancellationToken ct = default);
    Task<IReadOnlyList<Guid>> GetActiveSchoolIdsAsync(
        IReadOnlyCollection<Guid> schoolIds, CancellationToken ct = default);
    Task AddAsync(School school, CancellationToken ct = default);
    Task<Guid> AddCampusAsync(School school, CancellationToken ct = default);
    Task<Campus?> GetCampusAsync(Guid schoolId, Guid campusId, CancellationToken ct = default);
    Task<IReadOnlyList<Campus>> GetCampusesAsync(Guid schoolId, CancellationToken ct = default);
    Task<bool> IsCampusInUseAsync(Guid schoolId, Guid campusId, CancellationToken ct = default);
    Task AddCampusEntityAsync(Campus campus, CancellationToken ct = default);
    Task DeleteCampusAsync(Campus campus, CancellationToken ct = default);
}
