using Application.Common.Interfaces.Repositories;
using Domain.Entities.Billing;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class PaymentRepository : IPaymentRepository
{
    private readonly PlatformDbContext _context;

    public PaymentRepository(PlatformDbContext context) => _context = context;

    public Task<Payment?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        _context.Payments.FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted, ct);

    // Receipts currently use the payment GUID as their identifier.
    public Task<string?> GetLastReceiptNumberAsync(CancellationToken ct = default) =>
        Task.FromResult<string?>(null);

    public void Add(Payment payment) => _context.Payments.Add(payment);
}
