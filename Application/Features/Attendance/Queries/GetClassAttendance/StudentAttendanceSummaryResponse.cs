namespace Application.Features.Attendance.Queries.GetClassAttendance;

public record StudentAttendanceSummaryResponse(
    Guid   StudentEnrollmentId,
    string StudentName,
    int    TotalDays,
    int    PresentDays,
    int    AbsentDays,
    int    LateDays,
    decimal AttendancePercentage,
    List<StudentAttendanceSummaryDto> Records);
