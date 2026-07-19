using Domain.Enums;

namespace Application.Features.Portal.Queries.GetTeacherSchedule;

public record TeacherScheduleSlotDto(
    Guid         TeachingAssignmentId,
    string       SubjectName,
    string       ClassRoomName,
    string       GradeLevelName,
    string       RoomName,
    DayOfWeekEnum DayOfWeek,
    TimeSpan     StartTime,
    TimeSpan     EndTime);
