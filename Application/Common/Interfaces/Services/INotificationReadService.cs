using Application.Features.Notifications.Queries;

namespace Application.Common.Interfaces.Services;

public interface INotificationReadService
{
    Task<int> GetUnreadNotificationCountAsync(Guid userId, CancellationToken ct = default);
    Task<List<NotificationDto>> GetMyNotificationsAsync(Guid userId, int limit, CancellationToken ct = default);
}
