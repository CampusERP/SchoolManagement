using Domain.Entities.Tenancy;

namespace Application.Common.Interfaces.Repositories;

public interface ISchoolRegistrationRequestRepository
{
    Task AddAsync(SchoolRegistrationRequest request, CancellationToken ct = default);

    Task<SchoolRegistrationRequest?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>Returns the most recent pending request for the given desired subdomain, if any.</summary>
    Task<SchoolRegistrationRequest?> GetPendingBySubdomainAsync(string subdomainCode, CancellationToken ct = default);

    Task<bool> HasApprovedOrPendingForSubdomainAsync(string subdomainCode, CancellationToken ct = default);

    Task<(IReadOnlyList<SchoolRegistrationRequest> Items, int TotalCount)> GetPagedAsync(
        int page,
        int pageSize,
        string? search,
        string? status,
        CancellationToken ct = default);

    void Remove(SchoolRegistrationRequest request);
}
