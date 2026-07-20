using Application.Common.Interfaces.Services;
using Application.Common.Models;
using Application.Features.Portal.Queries.Shared;
using MediatR;

namespace Application.Features.Portal.Queries.GetStudentNotifications;

public class GetStudentNotificationsQueryHandler
    : IRequestHandler<GetStudentNotificationsQuery, Result<List<PortalNotificationDto>>>
{
    private readonly IPortalReadService _portalReadService;
    private readonly ICurrentUserService _user;

    public GetStudentNotificationsQueryHandler(IPortalReadService portalReadService, ICurrentUserService user)
    {
        _portalReadService = portalReadService;
        _user = user;
    }

    public async Task<Result<List<PortalNotificationDto>>> Handle(
        GetStudentNotificationsQuery request, CancellationToken ct)
    {
        var userId = _user.UserId;
        if (userId is null)
            return Result.Failure<List<PortalNotificationDto>>("Not authenticated.");

        var notifications = await _portalReadService.GetStudentNotificationsAsync(
            userId.Value, request.Limit, ct);

        return Result.Success(notifications);
    }
}
