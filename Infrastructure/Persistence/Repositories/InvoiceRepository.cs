using Application.Common.Interfaces.Repositories;
using Domain.Entities.Billing;

namespace Infrastructure.Persistence.Repositories;

public class InvoiceRepository : IInvoiceRepository
{
    public Task<Invoice?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        Task.FromResult<Invoice?>(null);

    public Task<string?> GetLastInvoiceNumberAsync(CancellationToken ct = default) =>
        Task.FromResult<string?>(null);

    public void Add(Invoice invoice) { }

    public void Update(Invoice invoice) { }
}
