using Application.Common.Interfaces.Services;
using Application.Common.Models;
using Application.Features.Portal.Queries.Shared;
using MediatR;

namespace Application.Features.Portal.Queries.GetParentBilling;

public class GetParentBillingQueryHandler
    : IRequestHandler<GetParentBillingQuery, Result<PagedResult<PortalInvoiceDto>>>
{
    private readonly IPortalReadService _portalReadService;

    public GetParentBillingQueryHandler(IPortalReadService portalReadService)
        => _portalReadService = portalReadService;

    public async Task<Result<PagedResult<PortalInvoiceDto>>> Handle(
        GetParentBillingQuery request, CancellationToken ct)
    {
        var p = request.Pagination ?? new PaginationParams();
        var result = await _portalReadService.GetParentBillingAsync(
            request.SchoolId, p, ct);

        return Result.Success(result);
    }
}
