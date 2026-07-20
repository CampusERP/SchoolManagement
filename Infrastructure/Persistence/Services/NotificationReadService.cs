using Application.Common.Interfaces.Services;
using Application.Features.Notifications.Queries;
using Domain.Entities.Notifications;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Services;

public class NotificationReadService : INotificationReadService
{
    private readonly ApplicationDbContext _db;

    public NotificationReadService(ApplicationDbContext db) => _db = db;

    public async Task<int> GetUnreadNotificationCountAsync(Guid userId, CancellationToken ct = default)
    {
        return await _db.Notifications.AsNoTracking()
            .Where(n => n.RecipientUserId == userId && n.Status == NotificationStatus.Delivered)
            .CountAsync(ct);
    }

    public async Task<List<NotificationDto>> GetMyNotificationsAsync(Guid userId, int limit, CancellationToken ct = default)
    {
        return await _db.Notifications.AsNoTracking()
            .Where(n => n.RecipientUserId == userId)
            .Join(_db.NotificationBatches,
                n => n.NotificationBatchId, b => b.Id,
                (n, b) => new { n, Batch = b })
            .Select(x => new NotificationDto(
                x.n.Id,
                x.Batch.Subject,
                x.Batch.Body,
                x.Batch.Channel.ToString(),
                x.n.Status.ToString(),
                x.Batch.CreatedAtUtc,
                x.n.ReadAtUtc))
            .OrderByDescending(x => x.CreatedAtUtc)
            .Take(limit)
            .ToListAsync(ct);
    }
}
