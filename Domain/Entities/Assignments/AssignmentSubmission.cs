using Domain.Common;
using Domain.Enums;
using Domain.Exceptions;

namespace Domain.Entities.Assignments;

/// <summary>
/// Represents a student's submission for an assignment. It tracks the submission status, grade, teacher feedback, and associated documents. This entity enforces domain rules related to submissions, such as preventing grading of unsubmitted assignments and ensuring that grades do not exceed the maximum score defined in the assignment.
/// </summary>

public class AssignmentSubmission : TenantEntity
{
    public Guid             AssignmentId        { get; private set; }
    public Guid             StudentEnrollmentId { get; private set; }
    public SubmissionStatus Status              { get; private set; }
    public decimal?         Grade               { get; private set; }
    public string?          TeacherFeedback     { get; private set; }
    public DateTime?        SubmittedAtUtc      { get; private set; }

    private readonly List<AssignmentSubmissionDocument> _documents = new();
    public IReadOnlyCollection<AssignmentSubmissionDocument> Documents => _documents.AsReadOnly();

    private AssignmentSubmission() { } // EF Core

    private AssignmentSubmission(Guid id, Guid schoolId, Guid assignmentId, Guid studentEnrollmentId, SubmissionStatus status)
        : base(id, schoolId)
    {
        AssignmentId        = assignmentId;
        StudentEnrollmentId = studentEnrollmentId;
        Status              = status;
        SubmittedAtUtc      = DateTime.UtcNow;
    }

    internal static AssignmentSubmission Create(Guid schoolId, Guid assignmentId, Guid studentEnrollmentId, SubmissionStatus status)
        => new(Guid.NewGuid(), schoolId, assignmentId, studentEnrollmentId, status);

    public void AttachDocument(Guid documentId)
    {
        if (_documents.Any(d => d.DocumentId == documentId))
            throw new DomainException("This document is already attached to this submission.");

        _documents.Add(AssignmentSubmissionDocument.Create(Id, documentId));
    }

    public void Mark(decimal grade, string? feedback, decimal? maxScore)
    {
        if (Status == SubmissionStatus.NotSubmitted)
            throw new DomainException("Cannot grade a submission that has not been submitted.");

        if (maxScore.HasValue && grade > maxScore.Value)
            throw new DomainException($"Grade ({grade}) cannot exceed max score ({maxScore.Value}).");

        Grade           = grade;
        TeacherFeedback = feedback;
        Status          = SubmissionStatus.Graded;
    }
}