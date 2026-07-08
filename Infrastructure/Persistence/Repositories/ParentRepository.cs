using Microsoft.EntityFrameworkCore;
using Application.Common.Interfaces.Repositories;
using Domain.Entities.People;

namespace Infrastructure.Persistence.Repositories;

public class ParentRepository : IParentRepository
{
    private readonly ApplicationDbContext _context;

    public ParentRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Parent?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await _context.Parents.FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task AddAsync(Parent parent, CancellationToken ct = default) =>
        await _context.Parents.AddAsync(parent, ct);
}
