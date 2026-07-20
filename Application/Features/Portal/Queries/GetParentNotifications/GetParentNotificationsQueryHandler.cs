using Application.Common.Interfaces.Services;
using Application.Common.Models;
using Application.Features.Portal.Queries.Shared;
using MediatR;

namespace Application.Features.Portal.Queries.GetParentNotifications;

public class GetParentNotificationsQueryHandler
    : IRequestHandler<GetParentNotificationsQuery, Result<List<PortalNotificationDto>>>
{
    private readonly IPortalReadService _portalReadService;
    private readonly ICurrentUserService _user;

    public GetParentNotificationsQueryHandler(IPortalReadService portalReadService, ICurrentUserService user)
    {
        _portalReadService = portalReadService;
        _user = user;
    }

    public async Task<Result<List<PortalNotificationDto>>> Handle(
        GetParentNotificationsQuery request, CancellationToken ct)
    {
        var userId = _user.UserId;
        if (userId is null)
            return Result.Failure<List<PortalNotificationDto>>("Not authenticated.");

        var notifications = await _portalReadService.GetParentNotificationsAsync(
            userId.Value, request.Limit, ct);

        return Result.Success(notifications);
    }
}
