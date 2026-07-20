using Application.Common.Interfaces;
using Application.Common.Interfaces.Services;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Portal.Queries.GetTeacherDashboard;

public class GetTeacherDashboardQueryHandler
    : IRequestHandler<GetTeacherDashboardQuery, Result<TeacherDashboardDto>>
{
    private readonly IPortalReadService _portalReadService;
    private readonly ICurrentUserService _user;

    public GetTeacherDashboardQueryHandler(IPortalReadService portalReadService, ICurrentUserService user)
    {
        _portalReadService = portalReadService;
        _user = user;
    }

    public async Task<Result<TeacherDashboardDto>> Handle(
        GetTeacherDashboardQuery request, CancellationToken ct)
    {
        var userId = _user.UserId;
        if (userId is null)
            return Result.Failure<TeacherDashboardDto>("Not authenticated.");

        var dashboard = await _portalReadService.GetTeacherDashboardAsync(
            request.SchoolId, userId.Value, ct);

        return Result.Success(dashboard);
    }
}
