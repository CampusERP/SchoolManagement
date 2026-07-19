using Application.Common.Interfaces.Services;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Notifications.Queries;

public class GetUnreadNotificationCountQueryHandler
    : IRequestHandler<GetUnreadNotificationCountQuery, Result<int>>
{
    private readonly INotificationReadService _readService;
    private readonly ICurrentUserService  _user;

    public GetUnreadNotificationCountQueryHandler(INotificationReadService readService, ICurrentUserService user)
    { _readService = readService; _user = user; }

    public async Task<Result<int>> Handle(
        GetUnreadNotificationCountQuery request, CancellationToken ct)
    {
        var userId = _user.UserId;
        if (userId is null) return Result.Success(0);

        var count = await _readService.GetUnreadNotificationCountAsync(userId.Value, ct);

        return Result.Success(count);
    }
}
