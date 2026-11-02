using Application.Common.Interfaces.Services;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Billing.Queries;

public class GetSubscriptionsQueryHandler
    : IRequestHandler<GetSubscriptionsQuery, Result<PagedResult<SubscriptionStatusDto>>>
{
    private readonly IBillingReadService _readService;
    public GetSubscriptionsQueryHandler(IBillingReadService readService) => _readService = readService;

    public async Task<Result<PagedResult<SubscriptionStatusDto>>> Handle(
        GetSubscriptionsQuery request, CancellationToken ct)
    {
        var p = request.Pagination ?? new PaginationParams();
        var result = await _readService.GetSubscriptionsAsync(p, request.SchoolId, request.Search, ct);
        return Result.Success(result);
    }
}
