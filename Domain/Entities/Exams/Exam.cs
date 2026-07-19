using Domain.Common;
using Domain.Exceptions;

namespace Domain.Entities.Exams;

public class Exam : TenantEntity, IAggregateRoot
{
    public Guid    SubjectId { get; private set; }
    public Guid    TermId    { get; private set; }
    public string  Name      { get; private set; } = default!;
    public decimal MaxScore  { get; private set; }
    public bool    IsLocked  { get; private set; }

    private readonly List<ExamSchedule> _schedules = new();
    public IReadOnlyCollection<ExamSchedule> Schedules => _schedules.AsReadOnly();

    private readonly List<ExamResult> _results = new();
    public IReadOnlyCollection<ExamResult> Results => _results.AsReadOnly();

    private Exam() { }

    private Exam(Guid id, Guid schoolId, Guid subjectId, Guid termId,
        string name, decimal maxScore) : base(id, schoolId)
    {
        SubjectId = subjectId;
        TermId    = termId;
        Name      = name;
        MaxScore  = maxScore;
    }

    public static Exam Create(Guid schoolId, Guid subjectId, Guid termId,
        string name, decimal maxScore)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Exam name is required.");
        if (maxScore <= 0)
            throw new DomainException("Max score must be greater than zero.");

        return new Exam(Guid.NewGuid(), schoolId, subjectId, termId, name, maxScore);
    }

    public ExamSchedule AddSchedule(Guid classRoomId, Guid roomId, DateTime examDate)
    {
        if (IsLocked)
            throw new DomainException("Cannot modify a locked exam.");
        if (_schedules.Any(s => s.ClassRoomId == classRoomId))
            throw new DomainException("This classroom already has a schedule for this exam.");

        var schedule = ExamSchedule.Create(Id, classRoomId, roomId, examDate);
        _schedules.Add(schedule);
        return schedule;
    }

    public ExamResult RecordResult(Guid examScheduleId, Guid studentEnrollmentId, decimal score)
    {
        if (IsLocked)
            throw new DomainException("Cannot add results to a locked exam.");
        if (score < 0 || score > MaxScore)
            throw new DomainException($"Score {score} is out of range (0–{MaxScore}).");

        var existing = _results.FirstOrDefault(r =>
            r.ExamScheduleId == examScheduleId &&
            r.StudentEnrollmentId == studentEnrollmentId);

        if (existing is not null)
        {
            existing.UpdateScore(score);
            return existing;
        }

        var result = ExamResult.Create(Id, examScheduleId, studentEnrollmentId, score);
        _results.Add(result);
        return result;
    }

    public void Lock()
    {
        if (!_results.Any())
            throw new DomainException("Cannot lock an exam with no results recorded.");
        IsLocked = true;
    }
}
