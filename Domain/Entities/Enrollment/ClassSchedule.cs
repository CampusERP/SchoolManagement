using Domain.Common;
using Domain.Enums;

namespace Domain.Entities.Enrollment;

/// <summary>
/// A recurring weekly timetable slot. Child of the TeachingAssignment
/// aggregate — created only via TeachingAssignment.AddSchedule.
/// </summary>
public class ClassSchedule : AuditableEntity
{
    public Guid TeachingAssignmentId { get; private set; }
    public Guid RoomId { get; private set; }
    public DayOfWeekEnum DayOfWeek { get; private set; }
    public TimeSpan StartTime { get; private set; }
    public TimeSpan EndTime { get; private set; }

    private ClassSchedule() { } // EF Core

    private ClassSchedule(Guid id, Guid teachingAssignmentId, Guid roomId,
        DayOfWeekEnum dayOfWeek, TimeSpan startTime, TimeSpan endTime) : base(id)
    {
        TeachingAssignmentId = teachingAssignmentId;
        RoomId = roomId;
        DayOfWeek = dayOfWeek;
        StartTime = startTime;
        EndTime = endTime;
    }

    internal static ClassSchedule Create(Guid teachingAssignmentId, Guid roomId,
        DayOfWeekEnum dayOfWeek, TimeSpan startTime, TimeSpan endTime)
    {
        return new ClassSchedule(Guid.NewGuid(), teachingAssignmentId, roomId, dayOfWeek, startTime, endTime);
    }
}
