using Application.Common.Interfaces.Repositories;
using Domain.Entities.Documents;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public sealed class DocumentAttachmentRepository : IDocumentAttachmentRepository
{
    private readonly ApplicationDbContext _db;
    public DocumentAttachmentRepository(ApplicationDbContext db) => _db = db;

    public async Task<IReadOnlyList<(DocumentAttachment Attachment, Document Document)>> GetByOwnerAsync(
        Guid ownerId, DocumentOwnerType ownerType, CancellationToken ct = default)
    {
        var pairs = await _db.DocumentAttachments.AsNoTracking()
            .Where(x => x.OwnerId == ownerId && x.OwnerType == ownerType)
            .Join(_db.Documents.AsNoTracking(), x => x.DocumentId, d => d.Id, (x, d) => new { x, d })
            .OrderByDescending(x => x.d.CreatedAtUtc)
            .ToListAsync(ct);
        return pairs.Select(x => (x.x, x.d)).ToList();
    }

    public Task<DocumentAttachment?> GetByDocumentAsync(Guid documentId, CancellationToken ct = default) =>
        _db.DocumentAttachments.FirstOrDefaultAsync(x => x.DocumentId == documentId, ct);

    public async Task AddAsync(DocumentAttachment attachment, CancellationToken ct = default) =>
        await _db.DocumentAttachments.AddAsync(attachment, ct);

    public void Remove(DocumentAttachment attachment) => _db.DocumentAttachments.Remove(attachment);
}
