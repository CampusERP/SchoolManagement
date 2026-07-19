using Application.Common.Interfaces.Services;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Notifications.Queries;

public class GetMyNotificationsQueryHandler
    : IRequestHandler<GetMyNotificationsQuery, Result<PagedResult<NotificationDto>>>
{
    private readonly INotificationReadService _readService;
    private readonly ICurrentUserService  _user;

    public GetMyNotificationsQueryHandler(INotificationReadService readService, ICurrentUserService user)
    { _readService = readService; _user = user; }

    public async Task<Result<PagedResult<NotificationDto>>> Handle(
        GetMyNotificationsQuery request, CancellationToken ct)
    {
        var p = request.Pagination ?? new PaginationParams();
        var userId = _user.UserId;
        if (userId is null)
            return Result.Failure<PagedResult<NotificationDto>>("Not authenticated.");

        var items = await _readService.GetMyNotificationsAsync(userId.Value, p.PageSize, ct);
        
        return Result.Success(new PagedResult<NotificationDto>(items, items.Count, p.Page, p.PageSize));
    }
}
