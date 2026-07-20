using Application.Common.Interfaces.Repositories;
using Domain.Entities.Billing;

namespace Infrastructure.Persistence.Repositories;

public class SubscriptionRepository : ISubscriptionRepository
{
    public Task<Subscription?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        Task.FromResult<Subscription?>(null);

    public Task<Subscription?> GetActiveBySchoolIdAsync(Guid schoolId, CancellationToken ct = default) =>
        Task.FromResult<Subscription?>(null);

    public void Add(Subscription subscription) { }

    public void Update(Subscription subscription) { }
}
