using Application.Common.Interfaces.Repositories;
using Domain.Entities.Notifications;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class NotificationBatchRepository : INotificationBatchRepository
{
    private readonly ApplicationDbContext _db;
    public NotificationBatchRepository(ApplicationDbContext db) => _db = db;

    public async Task<NotificationBatch?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await _db.NotificationBatches.FindAsync(new object[] { id }, ct);

    public async Task AddAsync(NotificationBatch batch, CancellationToken ct = default) =>
        await _db.NotificationBatches.AddAsync(batch, ct);
}
