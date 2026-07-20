using Application.Common.Interfaces.Services;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Portal.Queries.GetParentDashboard;

public class GetParentDashboardQueryHandler
    : IRequestHandler<GetParentDashboardQuery, Result<ParentDashboardDto>>
{
    private readonly IPortalReadService _portalReadService;
    private readonly ICurrentUserService _user;

    public GetParentDashboardQueryHandler(IPortalReadService portalReadService, ICurrentUserService user)
    {
        _portalReadService = portalReadService;
        _user = user;
    }

    public async Task<Result<ParentDashboardDto>> Handle(
        GetParentDashboardQuery request, CancellationToken ct)
    {
        var userId = _user.UserId;
        if (userId is null)
            return Result.Failure<ParentDashboardDto>("Not authenticated.");

        var dashboard = await _portalReadService.GetParentDashboardAsync(
            request.SchoolId, userId.Value, ct);

        return Result.Success(dashboard);
    }
}
