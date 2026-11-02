using Application.Common.Interfaces.Repositories;
using Domain.Entities.Billing;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class InvoiceRepository : IInvoiceRepository
{
    private readonly PlatformDbContext _context;

    public InvoiceRepository(PlatformDbContext context) => _context = context;

    public Task<Invoice?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        _context.Invoices
            .Include(i => i.Payments)
            .FirstOrDefaultAsync(i => i.Id == id && !i.IsDeleted, ct);

    // Invoices currently use the platform-generated GUID as their identifier.
    public Task<string?> GetLastInvoiceNumberAsync(CancellationToken ct = default) =>
        Task.FromResult<string?>(null);

    public void Add(Invoice invoice) => _context.Invoices.Add(invoice);

    public void Update(Invoice invoice) => _context.Invoices.Update(invoice);
}
