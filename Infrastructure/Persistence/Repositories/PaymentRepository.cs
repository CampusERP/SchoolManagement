using Application.Common.Interfaces.Repositories;
using Domain.Entities.Billing;

namespace Infrastructure.Persistence.Repositories;

public class PaymentRepository : IPaymentRepository
{
    public Task<Payment?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        Task.FromResult<Payment?>(null);

    public Task<string?> GetLastReceiptNumberAsync(CancellationToken ct = default) =>
        Task.FromResult<string?>(null);

    public void Add(Payment payment) { }
}
