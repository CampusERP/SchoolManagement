using Domain.Entities.Tenancy;

namespace Application.Common.Interfaces.Repositories;

public interface ISchoolRepository
{
    Task<School?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<School?> GetBySubdomainAsync(string subdomain, CancellationToken ct = default);
    Task AddAsync(School school, CancellationToken ct = default);
}
