using Domain.Entities.Documents;
using Domain.Enums;

namespace Application.Common.Interfaces.Repositories;

public interface IDocumentAttachmentRepository
{
    Task<IReadOnlyList<(DocumentAttachment Attachment, Document Document)>> GetByOwnerAsync(
        Guid ownerId, DocumentOwnerType ownerType, CancellationToken ct = default);
    Task<DocumentAttachment?> GetByDocumentAsync(Guid documentId, CancellationToken ct = default);
    Task AddAsync(DocumentAttachment attachment, CancellationToken ct = default);
    void Remove(DocumentAttachment attachment);
}
