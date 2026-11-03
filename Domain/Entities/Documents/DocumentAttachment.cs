using Domain.Common;
using Domain.Enums;

namespace Domain.Entities.Documents;

/// <summary>Connects a stored document to a student, teacher, or book.</summary>
public class DocumentAttachment : TenantEntity
{
    public Guid DocumentId { get; private set; }
    public Guid OwnerId { get; private set; }
    public DocumentOwnerType OwnerType { get; private set; }

    private DocumentAttachment() { }

    private DocumentAttachment(Guid id, Guid schoolId, Guid documentId, Guid ownerId, DocumentOwnerType ownerType)
        : base(id, schoolId)
    {
        DocumentId = documentId;
        OwnerId = ownerId;
        OwnerType = ownerType;
    }

    public static DocumentAttachment Create(Guid schoolId, Guid documentId, Guid ownerId, DocumentOwnerType ownerType) =>
        new(Guid.NewGuid(), schoolId, documentId, ownerId, ownerType);
}
