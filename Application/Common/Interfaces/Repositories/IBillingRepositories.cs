using Domain.Entities.Billing;

namespace Application.Common.Interfaces.Repositories;

public interface ISubscriptionPlanRepository
{
    Task<SubscriptionPlan?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<SubscriptionPlan>> GetAllAsync(bool activeOnly = false, CancellationToken ct = default);
    void Add(SubscriptionPlan plan);
    void Update(SubscriptionPlan plan);
}

public interface ISubscriptionRepository
{
    Task<Subscription?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Subscription?> GetActiveBySchoolIdAsync(Guid schoolId, CancellationToken ct = default);
    Task<Subscription?> GetLatestBySchoolIdAsync(Guid schoolId, CancellationToken ct = default);
    void Add(Subscription subscription);
    void Update(Subscription subscription);
}

public interface IPaymentRepository
{
    Task<Payment?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<string?> GetLastReceiptNumberAsync(CancellationToken ct = default);
    void Add(Payment payment);
}

public interface IInvoiceRepository
{
    Task<Invoice?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<string?> GetLastInvoiceNumberAsync(CancellationToken ct = default);
    void Add(Invoice invoice);
    void Update(Invoice invoice);
}
