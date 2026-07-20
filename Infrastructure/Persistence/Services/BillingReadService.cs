using Application.Common.Interfaces.Services;
using Application.Common.Models;
using Application.Features.Billing.Queries;

namespace Infrastructure.Persistence.Services;

public class BillingReadService : IBillingReadService
{
    public Task<List<SubscriptionPlanDto>> GetSubscriptionPlansAsync(CancellationToken ct = default) =>
        Task.FromResult(new List<SubscriptionPlanDto>());

    public Task<PagedResult<SubscriptionStatusDto>> GetSubscriptionsAsync(PaginationParams p, Guid? schoolId, CancellationToken ct = default) =>
        Task.FromResult(new PagedResult<SubscriptionStatusDto>(new List<SubscriptionStatusDto>(), 0, p.Page, p.PageSize));

    public Task<PagedResult<InvoiceListDto>> GetInvoicesAsync(PaginationParams p, Guid? schoolId, CancellationToken ct = default) =>
        Task.FromResult(new PagedResult<InvoiceListDto>(new List<InvoiceListDto>(), 0, p.Page, p.PageSize));

    public Task<PagedResult<PaymentDto>> GetPaymentsAsync(PaginationParams p, Guid? schoolId, CancellationToken ct = default) =>
        Task.FromResult(new PagedResult<PaymentDto>(new List<PaymentDto>(), 0, p.Page, p.PageSize));

    public Task<SubscriptionStatusDto?> GetSubscriptionStatusAsync(Guid schoolId, CancellationToken ct = default) =>
        Task.FromResult<SubscriptionStatusDto?>(null);
}
