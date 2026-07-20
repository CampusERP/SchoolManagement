using Application.Common.Interfaces.Services;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Billing.Queries;

public class GetSubscriptionPlansQueryHandler
    : IRequestHandler<GetSubscriptionPlansQuery, Result<List<SubscriptionPlanDto>>>
{
    private readonly IBillingReadService _readService;
    public GetSubscriptionPlansQueryHandler(IBillingReadService readService) => _readService = readService;

    public async Task<Result<List<SubscriptionPlanDto>>> Handle(
        GetSubscriptionPlansQuery request, CancellationToken ct)
    {
        var items = await _readService.GetSubscriptionPlansAsync(ct);
        return Result.Success(items);
    }
}
