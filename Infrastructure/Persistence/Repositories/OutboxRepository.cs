using Application.Common.Interfaces.Repositories;
using Domain.Entities.Outbox;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class OutboxRepository : IOutboxRepository
{
    private readonly PlatformDbContext _db;
    public OutboxRepository(PlatformDbContext db) => _db = db;

    public async Task<List<OutboxMessage>> GetPendingAsync(int batchSize, CancellationToken ct = default) =>
        await _db.OutboxMessages
            .Where(m => !m.ProcessedAtUtc.HasValue)
            .OrderBy(m => m.CreatedAtUtc)
            .Take(batchSize)
            .ToListAsync(ct);

    public async Task AddAsync(OutboxMessage message, CancellationToken ct = default) =>
        await _db.OutboxMessages.AddAsync(message, ct);
}
