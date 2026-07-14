using Microsoft.EntityFrameworkCore;
using Application.Common.Interfaces.Repositories;
using Domain.Entities.Tenancy;

namespace Infrastructure.Persistence.Repositories;

public class UserSchoolMembershipRepository : IUserSchoolMembershipRepository
{
    private readonly PlatformDbContext _context;

    public UserSchoolMembershipRepository(PlatformDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(UserSchoolMembership userSchoolMembership, CancellationToken ct = default)
    {
        await _context.UserSchoolMemberships.AddAsync(userSchoolMembership, ct);
    }

    public async Task<UserSchoolMembership?> GetAsync(Guid userId, Guid schoolId, CancellationToken ct)
    {
        return await _context.UserSchoolMemberships.FirstOrDefaultAsync(x => 
        x.ApplicationUserId == userId && x.SchoolId == schoolId, ct);
    }

    public async Task<IReadOnlyList<UserSchoolMembership>> GetActiveMembershipsAsync(Guid userId, CancellationToken ct)
    {
        return await _context.UserSchoolMemberships.AsNoTracking()
            .Where(x => x.ApplicationUserId == userId && x.IsActive).ToListAsync(ct);
    }
}
