using Application.Common.Interfaces.Services;
using Application.Common.Models;
using Application.Features.Portal.Queries.Shared;
using MediatR;

namespace Application.Features.Portal.Queries.GetParentChildReportCards;

public class GetParentChildReportCardsQueryHandler
    : IRequestHandler<GetParentChildReportCardsQuery, Result<PagedResult<PortalReportCardDto>>>
{
    private readonly IPortalReadService _portalReadService;
    private readonly ICurrentUserService _user;

    public GetParentChildReportCardsQueryHandler(IPortalReadService portalReadService, ICurrentUserService user)
    {
        _portalReadService = portalReadService;
        _user = user;
    }

    public async Task<Result<PagedResult<PortalReportCardDto>>> Handle(
        GetParentChildReportCardsQuery request, CancellationToken ct)
    {
        var userId = _user.UserId;
        if (userId is null)
            return Result.Failure<PagedResult<PortalReportCardDto>>("Not authenticated.");

        var p = request.Pagination ?? new PaginationParams();
        var result = await _portalReadService.GetParentChildReportCardsAsync(
            request.SchoolId, userId.Value, request.StudentId, p, ct);

        return Result.Success(result);
    }
}
