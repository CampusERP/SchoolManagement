using Domain.Entities.People;

namespace Application.Common.Interfaces.Repositories;

public interface ISchoolAdminProfileRepository
{
    Task AddAsync(SchoolAdminProfile schoolAdminProfile, CancellationToken ct = default);
    Task<SchoolAdminProfile?> GetByUserIdAsync(Guid userId, CancellationToken ct);
}