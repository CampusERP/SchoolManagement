using Domain.Enums;

namespace Application.Features.Enrollment.Commands.AssignTeacher;

public record ScheduleSlot(DayOfWeekEnum DayOfWeek, TimeSpan StartTime, TimeSpan EndTime, Guid RoomId);
