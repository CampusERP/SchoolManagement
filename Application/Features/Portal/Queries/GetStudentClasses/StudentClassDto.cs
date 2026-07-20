using Domain.Enums;

namespace Application.Features.Portal.Queries.GetStudentClasses;

public record StudentClassDto(
    Guid   TeachingAssignmentId,
    string SubjectName,
    string SubjectCode,
    string TeacherFirstName,
    string TeacherLastName,
    string ClassRoomName,
    List<StudentClassScheduleSlot> ScheduleSlots);

public record StudentClassScheduleSlot(
    DayOfWeekEnum DayOfWeek,
    TimeSpan     StartTime,
    TimeSpan     EndTime,
    string       RoomName);
