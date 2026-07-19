using Domain.Common;
using Domain.Enums;
using Domain.Exceptions;

namespace Domain.Entities.Assignments;

/// <summary>
/// Represents an assignment that is part of a teaching assignment. It contains details about the assignment, such as title, instructions, due date, and maximum score. It also manages submissions from students and enforces domain rules related to assignment updates and submissions.
/// </summary>

public class Assignment : TenantEntity, IAggregateRoot
{
    public Guid TeachingAssignmentId { get; private set; }
    public string Title { get; private set; } = default!;
    public string? Instructions { get; private set; }
    public DateTime DueDate { get; private set; }
    public decimal? MaxScore { get; private set; }

    private readonly List<AssignmentSubmission> _submissions = new();
    public IReadOnlyCollection<AssignmentSubmission> Submissions => _submissions.AsReadOnly();

    private Assignment() { } // EF Core

    private Assignment(Guid id, Guid schoolId, Guid teachingAssignmentId,
        string title, string? instructions, DateTime dueDate, decimal? maxScore)
        : base(id, schoolId)
    {
        TeachingAssignmentId = teachingAssignmentId;
        Title = title;
        Instructions = instructions;
        DueDate = dueDate;
        MaxScore = maxScore;
    }

    public static Assignment Create(Guid schoolId, Guid teachingAssignmentId,
        string title, string? instructions, DateTime dueDate, decimal? maxScore = null)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Assignment title is required.", nameof(title));

        return new Assignment(Guid.NewGuid(), schoolId, teachingAssignmentId,
            title, instructions, dueDate, maxScore);
    }

    public AssignmentSubmission Submit(Guid studentEnrollmentId)
    {
        var existing = _submissions.FirstOrDefault(s => s.StudentEnrollmentId == studentEnrollmentId);
        if (existing is not null)
            throw new DomainException("This student has already submitted this assignment.");

        var isLate = DateTime.UtcNow > DueDate;
        var status = isLate ? SubmissionStatus.Late : SubmissionStatus.Submitted;
        var sub = AssignmentSubmission.Create(Id, studentEnrollmentId, status);
        _submissions.Add(sub);
        return sub;
    }

    public void UpdateDetails(string title, string? instructions, DateTime dueDate)
    {
        if (_submissions.Any(s => s.Status != SubmissionStatus.NotSubmitted))
            throw new DomainException("Cannot change an assignment that already has submissions.");

        Title = title;
        Instructions = instructions;
        DueDate = dueDate;
    }
}
