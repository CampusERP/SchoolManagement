using Domain.Common;

namespace Domain.Entities.Exams;

public class ReportCardSubjectResult : AuditableEntity
{
    public Guid    ReportCardId { get; private set; }
    public Guid    SubjectId    { get; private set; }
    public string  SubjectName  { get; private set; } = default!;
    public decimal Score        { get; private set; }
    public decimal MaxScore     { get; private set; }
    public string  Grade        { get; private set; } = default!;

    private ReportCardSubjectResult() { }

    private ReportCardSubjectResult(Guid id, Guid reportCardId, Guid subjectId,
        string subjectName, decimal score, decimal maxScore, string grade) : base(id)
    {
        ReportCardId = reportCardId;
        SubjectId    = subjectId;
        SubjectName  = subjectName;
        Score        = score;
        MaxScore     = maxScore;
        Grade        = grade;
    }

    internal static ReportCardSubjectResult Create(Guid reportCardId, Guid subjectId,
        string subjectName, decimal score, decimal maxScore, string grade)
        => new(Guid.NewGuid(), reportCardId, subjectId, subjectName, score, maxScore, grade);
}
