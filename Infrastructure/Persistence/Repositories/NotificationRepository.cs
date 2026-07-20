using Application.Common.Interfaces.Repositories;
using Domain.Entities.Notifications;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class NotificationRepository : INotificationRepository
{
    private readonly ApplicationDbContext _db;
    public NotificationRepository(ApplicationDbContext db) => _db = db;

    public async Task<Notification?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await _db.Notifications.FindAsync(new object[] { id }, ct);

    public async Task<List<Notification>> GetUnreadForUserAsync(Guid userId, CancellationToken ct = default) =>
        await _db.Notifications
            .Where(n => n.RecipientUserId == userId && n.Status == NotificationStatus.Pending)
            .OrderByDescending(n => n.CreatedAtUtc)
            .ToListAsync(ct);

    public void Update(Notification notification) => _db.Notifications.Update(notification);
}
