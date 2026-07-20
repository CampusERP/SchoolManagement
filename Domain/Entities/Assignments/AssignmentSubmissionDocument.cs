using Domain.Common;

namespace Domain.Entities.Assignments;

public class AssignmentSubmissionDocument : AuditableEntity
{
    public Guid AssignmentSubmissionId { get; private set; }
    public Guid DocumentId             { get; private set; }

    private AssignmentSubmissionDocument() { } // EF Core

    private AssignmentSubmissionDocument(Guid id, Guid submissionId, Guid documentId) : base(id)
    {
        AssignmentSubmissionId = submissionId;
        DocumentId             = documentId;
    }

    internal static AssignmentSubmissionDocument Create(Guid submissionId, Guid documentId)
        => new(Guid.NewGuid(), submissionId, documentId);
}
