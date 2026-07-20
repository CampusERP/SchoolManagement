using Domain.Common;

namespace Domain.Entities.Exams;

/// <summary>
/// Represents the result of an exam for a specific student enrollment, including the exam ID, schedule ID, score, and any remarks.
/// </summary>

public class ExamResult : TenantEntity
{
    public Guid    ExamId              { get; private set; }  // FK to Exam aggregate root
    public Guid    ExamScheduleId      { get; private set; }  // which schedule (classroom + date)
    public Guid    StudentEnrollmentId { get; private set; }
    public decimal Score               { get; private set; }
    public string? Remarks             { get; private set; }

    private ExamResult() { }

    private ExamResult(Guid id, Guid schoolId, Guid examId, Guid examScheduleId,
        Guid studentEnrollmentId, decimal score) : base(id, schoolId)
    {
        ExamId              = examId;
        ExamScheduleId      = examScheduleId;
        StudentEnrollmentId = studentEnrollmentId;
        Score               = score;
    }

    internal static ExamResult Create(Guid schoolId, Guid examId, Guid scheduleId,
        Guid enrollmentId, decimal score)
        => new(Guid.NewGuid(), schoolId, examId, scheduleId, enrollmentId, score);

    internal void UpdateScore(decimal score) => Score = score;

    public void AddRemarks(string remarks) => Remarks = remarks;
}
