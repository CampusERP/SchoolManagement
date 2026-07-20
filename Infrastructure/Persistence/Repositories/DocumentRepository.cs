using Application.Common.Interfaces.Repositories;
using Domain.Entities.Documents;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class DocumentRepository : IDocumentRepository
{
    private readonly ApplicationDbContext _db;
    public DocumentRepository(ApplicationDbContext db) => _db = db;

    public async Task<Document?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await _db.Documents.FindAsync(new object[] { id }, ct);

    public async Task AddAsync(Document document, CancellationToken ct = default) =>
        await _db.Documents.AddAsync(document, ct);
}
