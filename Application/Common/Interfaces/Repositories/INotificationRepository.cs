using Domain.Entities.Notifications;

namespace Application.Common.Interfaces.Repositories;

public interface INotificationRepository
{
    Task<Notification?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<Notification>> GetUnreadForUserAsync(Guid userId, CancellationToken ct = default);
    void Update(Notification notification);
}
