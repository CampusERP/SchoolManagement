using Application.Common.Interfaces.Repositories;
using Domain.Entities.Tenancy;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class SchoolRegistrationRequestRepository : ISchoolRegistrationRequestRepository
{
    private readonly PlatformDbContext _context;

    public SchoolRegistrationRequestRepository(PlatformDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(SchoolRegistrationRequest request, CancellationToken ct = default)
        => await _context.SchoolRegistrationRequests.AddAsync(request, ct);

    public Task<SchoolRegistrationRequest?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _context.SchoolRegistrationRequests.FirstOrDefaultAsync(r => r.Id == id, ct);

    public Task<SchoolRegistrationRequest?> GetPendingBySubdomainAsync(string subdomainCode, CancellationToken ct = default)
        => _context.SchoolRegistrationRequests
            .Where(r => r.SubdomainCode == subdomainCode && r.Status == SchoolRegistrationRequestStatus.Pending)
            .OrderByDescending(r => r.CreatedAtUtc)
            .FirstOrDefaultAsync(ct);

    public Task<bool> HasApprovedOrPendingForSubdomainAsync(string subdomainCode, CancellationToken ct = default)
        => _context.SchoolRegistrationRequests
            .AnyAsync(r => r.SubdomainCode == subdomainCode &&
                           (r.Status == SchoolRegistrationRequestStatus.Pending ||
                            r.Status == SchoolRegistrationRequestStatus.Approved), ct);

    public async Task<(IReadOnlyList<SchoolRegistrationRequest> Items, int TotalCount)> GetPagedAsync(
        int page,
        int pageSize,
        string? search,
        string? status,
        CancellationToken ct = default)
    {
        var query = _context.SchoolRegistrationRequests.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(r => r.Status == status);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var terms = search
                .Trim()
                .ToLower()
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Distinct()
                .Take(5)
                .ToArray();

            foreach (var term in terms)
            {
                query = query.Where(r =>
                    r.SchoolName.ToLower().Contains(term) ||
                    r.SubdomainCode.ToLower().Contains(term) ||
                    r.AdminEmail.ToLower().Contains(term) ||
                    r.AdminFirstName.ToLower().Contains(term) ||
                    r.AdminLastName.ToLower().Contains(term) ||
                    (r.SchoolEmail != null && r.SchoolEmail.ToLower().Contains(term)));
            }
        }

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(r => r.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, total);
    }

    public void Remove(SchoolRegistrationRequest request)
        => _context.SchoolRegistrationRequests.Remove(request);
}
