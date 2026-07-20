using Domain.Entities.Notifications;

namespace Application.Common.Interfaces.Repositories;

public interface INotificationBatchRepository
{
    Task<NotificationBatch?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(NotificationBatch batch, CancellationToken ct = default);
}
