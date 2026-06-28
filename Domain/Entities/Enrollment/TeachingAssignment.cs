using Domain.Common;
using Domain.Enums;
using Domain.Exceptions;

namespace Domain.Entities.Enrollment;

/// <summary>
/// Teacher assigned to teach a Subject in a ClassRoom for a Term. Owns
/// ClassSchedule entries — clash detection (no double-booking the same
/// teacher in the same time slot) belongs here, not in Application code,
/// so the rule is enforced no matter which entry point calls it.
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
}
