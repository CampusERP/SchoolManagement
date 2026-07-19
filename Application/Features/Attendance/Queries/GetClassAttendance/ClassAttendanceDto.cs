namespace Application.Features.Attendance.Queries.GetClassAttendance;

public record ClassAttendanceDto(
    Guid SessionId,
    DateOnly Date,
    bool IsLocked,
    List<StudentAttendanceDto> Records);
