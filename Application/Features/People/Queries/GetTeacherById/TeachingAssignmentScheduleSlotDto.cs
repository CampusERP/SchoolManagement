namespace Application.Features.People.Queries.GetTeacherById;

public record TeachingAssignmentScheduleSlotDto(
    Guid Id,
    int DayOfWeek,
    string StartTime,
    string EndTime,
    Guid RoomId,
    string RoomName);
