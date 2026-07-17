using Domain.Entities.Tenancy;

namespace Application.Common.Interfaces.Repositories;

public interface IUserSchoolMembershipRepository
{
    Task AddAsync(UserSchoolMembership userSchoolMembership, CancellationToken ct = default);
    Task<UserSchoolMembership?> GetAsync(Guid userId, Guid schoolId, CancellationToken ct);
    Task<IReadOnlyList<UserSchoolMembership>> GetActiveMembershipsAsync(Guid userId, CancellationToken ct);
}