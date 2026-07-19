using Domain.Common;

namespace Domain.Entities.Exams;

/// <summary>
/// Represents a scheduled exam for a specific classroom and room.
/// </summary>
public class ExamSchedule : Entity
{
    public Guid     ExamId      { get; private set; } // FK to Exam aggregate root
    public Guid     ClassRoomId { get; private set; } // Which classroom is taking this exam
    public Guid     RoomId      { get; private set; } // Physical room where it takes place
    public DateTime ExamDate    { get; private set; }

    private ExamSchedule() { }

    private ExamSchedule(Guid id, Guid examId, Guid classRoomId, Guid roomId, DateTime examDate) : base(id)
    {
        ExamId      = examId;
        ClassRoomId = classRoomId;
        RoomId      = roomId;
        ExamDate    = examDate;
    }

    internal static ExamSchedule Create(Guid examId, Guid classRoomId, Guid roomId, DateTime examDate)
        => new(Guid.NewGuid(), examId, classRoomId, roomId, examDate);
}
