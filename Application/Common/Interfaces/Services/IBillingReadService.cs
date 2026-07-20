using Application.Common.Models;
using Application.Features.Billing.Queries;

namespace Application.Common.Interfaces.Services;

public interface IBillingReadService
{
    Task<List<SubscriptionPlanDto>> GetSubscriptionPlansAsync(CancellationToken ct = default);
    Task<PagedResult<SubscriptionStatusDto>> GetSubscriptionsAsync(PaginationParams p, Guid? schoolId, CancellationToken ct = default);
    Task<PagedResult<InvoiceListDto>> GetInvoicesAsync(PaginationParams p, Guid? schoolId, CancellationToken ct = default);
    Task<PagedResult<PaymentDto>> GetPaymentsAsync(PaginationParams p, Guid? schoolId, CancellationToken ct = default);
    Task<SubscriptionStatusDto?> GetSubscriptionStatusAsync(Guid schoolId, CancellationToken ct = default);
}
