using Application.Common.Interfaces.Repositories;
using Domain.Entities.Billing;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class SubscriptionPlanRepository : ISubscriptionPlanRepository
{
    private readonly PlatformDbContext _context;

    public SubscriptionPlanRepository(PlatformDbContext context) => _context = context;

    public Task<SubscriptionPlan?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        _context.SubscriptionPlans
            .Include(p => p.Benefits)
            .FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<IReadOnlyList<SubscriptionPlan>> GetAllAsync(
        bool activeOnly = false, CancellationToken ct = default)
    {
        var query = _context.SubscriptionPlans.AsNoTracking();
        if (activeOnly) query = query.Where(p => p.IsActive);

        return await query.OrderBy(p => p.PriceMonthly).ToListAsync(ct);
    }

    public void Add(SubscriptionPlan plan) => _context.SubscriptionPlans.Add(plan);

    public void Update(SubscriptionPlan plan)
    {
        // Plans are loaded through this context, so their scalar and benefit
        // changes are already tracked. Calling Update on the graph would mark
        // newly added benefits as Modified instead of Added.
        if (_context.Entry(plan).State == EntityState.Detached)
            _context.SubscriptionPlans.Update(plan);
    }
}
