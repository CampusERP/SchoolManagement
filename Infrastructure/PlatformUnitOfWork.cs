using Application.Common.Interfaces;
using Infrastructure.Persistence;

namespace Infrastructure;

public class PlatformUnitOfWork : IPlatformUnitOfWork
{
    private readonly PlatformDbContext _context;

    public PlatformUnitOfWork(PlatformDbContext context)
    {
        _context = context;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => _context.SaveChangesAsync(cancellationToken);
}