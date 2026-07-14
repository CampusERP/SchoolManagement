using Application.Common.Interfaces.Repositories;
using Domain.Entities.People;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class SchoolAdminProfileRepository : ISchoolAdminProfileRepository
{
    private readonly ApplicationDbContext _context;

    public SchoolAdminProfileRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(SchoolAdminProfile schoolAdminProfile, CancellationToken ct = default)
    {
        await _context.SchoolAdminProfiles.AddAsync(schoolAdminProfile, ct);
    }

    public async Task<SchoolAdminProfile?> GetByUserIdAsync(Guid userId, CancellationToken ct)
    {
        return await _context.SchoolAdminProfiles.FirstOrDefaultAsync(s => s.ApplicationUserId == userId, ct);
    }
}