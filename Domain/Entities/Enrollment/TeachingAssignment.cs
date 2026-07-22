using Domain.Common;
using Domain.Enums;
using Domain.Exceptions;

namespace Domain.Entities.Enrollment;

/// <summary>
/// Represents the assignment of a teacher to a subject and class for a specific term, along with the associated class schedules.
/// </summary>
public class TeachingAssignment : TenantEntity, IAggregateRoot
{
    public Guid TeacherId { get; private set; }
    public Guid SubjectId { get; private set; }
    public Guid ClassRoomId { get; private set; }
    public Guid TermId { get; private set; }

    private readonly List<ClassSchedule> _schedules = new();
    public IReadOnlyCollection<ClassSchedule> Schedules => _schedules.AsReadOnly();

    private TeachingAssignment() { } // EF Core

    private TeachingAssignment(Guid id, Guid schoolId, Guid teacherId, Guid subjectId,
        Guid classRoomId, Guid termId) : base(id, schoolId)
    {
        TeacherId = teacherId;
        SubjectId = subjectId;
        ClassRoomId = classRoomId;
        TermId = termId;
    }

    public static TeachingAssignment Create(Guid schoolId, Guid teacherId, Guid subjectId,
        Guid classRoomId, Guid termId)
    {
        return new TeachingAssignment(Guid.NewGuid(), schoolId, teacherId, subjectId, classRoomId, termId);
    }

    /// <summary>
    /// Adds a weekly recurring slot. Caller (the command handler) is
    /// responsible for checking other TeachingAssignments for the same
    /// teacher/room across the school before calling this — this method
    /// only enforces the invariant within THIS assignment's own schedules.
    /// </summary>
    public ClassSchedule AddSchedule(Guid roomId, DayOfWeekEnum dayOfWeek, TimeSpan startTime, TimeSpan endTime)
    {
        if (endTime <= startTime)
            throw new DomainException("Schedule end time must be after the start time.");

        bool overlaps = _schedules.Any(s =>
            s.DayOfWeek == dayOfWeek &&
            startTime < s.EndTime &&
            endTime > s.StartTime);

        if (overlaps)
            throw new DomainException("This teaching assignment already has an overlapping schedule slot.");

        var schedule = ClassSchedule.Create(Id, roomId, dayOfWeek, startTime, endTime);
        _schedules.Add(schedule);
        return schedule;
    }

    public void RemoveSchedule(Guid scheduleId)
    {
        var schedule = _schedules.FirstOrDefault(s => s.Id == scheduleId);
        if (schedule is not null)
            _schedules.Remove(schedule);
    }

    public void ClearSchedules()
    {
        _schedules.Clear();
    }

    public void UpdateSubject(Guid subjectId) => SubjectId = subjectId;
    public void UpdateClassRoom(Guid classRoomId) => ClassRoomId = classRoomId;
    public void UpdateTerm(Guid termId) => TermId = termId;
}
