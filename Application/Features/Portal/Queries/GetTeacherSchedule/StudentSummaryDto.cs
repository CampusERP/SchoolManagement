namespace Application.Features.Portal.Queries.GetTeacherSchedule;

public record StudentSummaryDto(
    string StudentName,
    string ClassRoomName,
    string GradeLevelName,
    string AcademicYearName,
    decimal AttendancePercentage,
    int PendingAssignments);
