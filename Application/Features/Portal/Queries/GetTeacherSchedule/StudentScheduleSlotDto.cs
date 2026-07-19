using Domain.Enums;

namespace Application.Features.Portal.Queries.GetTeacherSchedule;

public record StudentScheduleSlotDto(
    string        SubjectName,
    string        TeacherFirstName,
    string        TeacherLastName,
    string        RoomName,
    DayOfWeekEnum DayOfWeek,
    TimeSpan      StartTime,
    TimeSpan      EndTime);
