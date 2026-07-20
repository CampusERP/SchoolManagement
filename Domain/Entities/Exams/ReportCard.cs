using Domain.Common;
using Domain.Exceptions;

namespace Domain.Entities.Exams;

/// <summary>
/// Represents a report card for a student enrollment in a specific term, including overall percentage, grade, and subject results.
/// </summary>
public class ReportCard : TenantEntity, IAggregateRoot
{
    public Guid     StudentEnrollmentId { get; private set; }
    public Guid     TermId              { get; private set; }
    public decimal  OverallPercentage   { get; private set; }
    public string   OverallGrade        { get; private set; } = default!;
    public bool     IsLocked            { get; private set; }
    public DateTime GeneratedAtUtc      { get; private set; }
    public Guid     GeneratedByUserId   { get; private set; }

    private readonly List<ReportCardSubjectResult> _subjectResults = new();
    public IReadOnlyCollection<ReportCardSubjectResult> SubjectResults => _subjectResults.AsReadOnly();

    private ReportCard() { }

    private ReportCard(Guid id, Guid schoolId, Guid studentEnrollmentId,
        Guid termId, decimal overallPercentage, string overallGrade,
        Guid generatedByUserId) : base(id, schoolId)
    {
        StudentEnrollmentId = studentEnrollmentId;
        TermId              = termId;
        OverallPercentage   = overallPercentage;
        OverallGrade        = overallGrade;
        GeneratedAtUtc      = DateTime.UtcNow;
        GeneratedByUserId   = generatedByUserId;
        IsLocked            = false;
    }

    public static ReportCard Generate(Guid schoolId, Guid studentEnrollmentId,
        Guid termId, decimal overallPercentage, string overallGrade, Guid generatedByUserId,
        IEnumerable<(Guid SubjectId, string SubjectName, decimal Score, decimal MaxScore, string Grade)> subjects)
    {
        var card = new ReportCard(Guid.NewGuid(), schoolId, studentEnrollmentId,
            termId, overallPercentage, overallGrade, generatedByUserId);

        foreach (var s in subjects)
            card._subjectResults.Add(ReportCardSubjectResult.Create(
                card.Id, s.SubjectId, s.SubjectName, s.Score, s.MaxScore, s.Grade));

        return card;
    }

    public void Lock()
    {
        if (IsLocked)
            throw new DomainException("Report card is already locked.");
        IsLocked = true;
    }
}