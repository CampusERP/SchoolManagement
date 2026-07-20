using Application.Common.Interfaces.Repositories;
using Domain.Entities.Billing;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class SubscriptionPlanRepository : ISubscriptionPlanRepository
{
    public Task<SubscriptionPlan?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        Task.FromResult<SubscriptionPlan?>(null);

    public void Add(SubscriptionPlan plan) { }
}
