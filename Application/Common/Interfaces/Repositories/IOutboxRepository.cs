using Domain.Entities.Outbox;

namespace Application.Common.Interfaces.Repositories;

public interface IOutboxRepository
{
    Task<List<OutboxMessage>> GetPendingAsync(int batchSize, CancellationToken ct = default);
    Task AddAsync(OutboxMessage message, CancellationToken ct = default);
}
