using Domain.Enums;

namespace Application.Features.Attendance.Queries.GetClassAttendance;

public record StudentAttendanceDto(
    Guid StudentEnrollmentId,
    string StudentFirstName,
    string StudentLastName,
    string StudentCode,
    AttendanceStatus Status,
    string? Note);
