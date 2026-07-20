using Application.Common.Interfaces.Services;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Billing.Queries;

public class GetSubscriptionStatusQueryHandler
    : IRequestHandler<GetSubscriptionStatusQuery, Result<SubscriptionStatusDto>>
{
    private readonly IBillingReadService _readService;
    public GetSubscriptionStatusQueryHandler(IBillingReadService readService) => _readService = readService;

    public async Task<Result<SubscriptionStatusDto>> Handle(
        GetSubscriptionStatusQuery request, CancellationToken ct)
    {
        var dto = await _readService.GetSubscriptionStatusAsync(request.SchoolId, ct);

        if (dto is null)
            return Result.Failure<SubscriptionStatusDto>("No active subscription found.");

        return Result.Success(dto);
    }
}
