using Domain.Entities.Documents;

namespace Application.Common.Interfaces.Repositories;

public interface IDocumentRepository
{
    Task<Document?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(Document document, CancellationToken ct = default);
}
