using Application.Common.Interfaces.Repositories;
using Domain.Entities.Billing;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class SubscriptionRepository : ISubscriptionRepository
{
    private readonly PlatformDbContext _context;

    public SubscriptionRepository(PlatformDbContext context) => _context = context;

    public Task<Subscription?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        _context.Subscriptions
            .Include(s => s.Invoices)
            .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted, ct);

    public Task<Subscription?> GetActiveBySchoolIdAsync(Guid schoolId, CancellationToken ct = default) =>
        _context.Subscriptions
            .Include(s => s.Invoices)
            .Where(s => s.SchoolId == schoolId && !s.IsDeleted &&
                        s.Status == SubscriptionStatus.Active)
            .OrderByDescending(s => s.CreatedAtUtc)
            .FirstOrDefaultAsync(ct);

    public Task<Subscription?> GetLatestBySchoolIdAsync(Guid schoolId, CancellationToken ct = default) =>
        _context.Subscriptions
            .Include(s => s.Invoices)
            .Where(s => s.SchoolId == schoolId && !s.IsDeleted)
            .OrderByDescending(s => s.CreatedAtUtc)
            .FirstOrDefaultAsync(ct);

    public void Add(Subscription subscription) => _context.Subscriptions.Add(subscription);

    public void Update(Subscription subscription) => _context.Subscriptions.Update(subscription);
}
