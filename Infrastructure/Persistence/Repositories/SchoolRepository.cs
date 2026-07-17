using Application.Common.Interfaces.Repositories;
using Domain.Entities.Tenancy;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class SchoolRepository : ISchoolRepository
{
    private readonly PlatformDbContext _context;

    public SchoolRepository(PlatformDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(School school, CancellationToken ct = default)
    {
        await _context.Schools.AddAsync(school, ct);
    }

    public async Task<School?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Schools.FindAsync(id, ct);
    }

    public async Task<School?> GetBySubdomainAsync(string subdomain, CancellationToken ct = default)
    {
        return await _context.Schools.FirstOrDefaultAsync(s => s.SubdomainCode == subdomain, ct);
    }
}
